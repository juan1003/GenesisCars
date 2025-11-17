using System.Linq;
using GenesisCars.Application.Exceptions;
using GenesisCars.Application.Marketplace;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Application.Payments;

public sealed class PaymentService : IPaymentService
{
  private readonly IPaymentIntentRepository _paymentIntentRepository;
  private readonly IMarketplaceListingRepository _listingRepository;
  private readonly ICarRepository _carRepository;
  private readonly IPaymentGateway _paymentGateway;
  private readonly IUnitOfWork _unitOfWork;

  public PaymentService(
      IPaymentIntentRepository paymentIntentRepository,
      IMarketplaceListingRepository listingRepository,
      ICarRepository carRepository,
      IPaymentGateway paymentGateway,
      IUnitOfWork unitOfWork)
  {
    _paymentIntentRepository = paymentIntentRepository;
    _listingRepository = listingRepository;
    _carRepository = carRepository;
    _paymentGateway = paymentGateway;
    _unitOfWork = unitOfWork;
  }

  public async Task<PaymentIntentDto> CreateAsync(CreatePaymentIntentRequest request, CancellationToken cancellationToken = default)
  {
    var listing = await _listingRepository.GetByIdAsync(request.ListingId, cancellationToken).ConfigureAwait(false);
    if (listing is null)
    {
      throw new NotFoundException($"Listing '{request.ListingId}' was not found.");
    }

    if (listing.Status != MarketplaceListingStatus.Active)
    {
      throw new ConflictException("Only active listings can accept payments.");
    }

    var amount = listing.AskingPrice;
    var paymentIntent = PaymentIntent.Create(listing.Id, amount, request.Currency);

    var car = await _carRepository.GetByIdAsync(listing.CarId, cancellationToken).ConfigureAwait(false);
    var description = car is null
        ? $"Listing {listing.Id} payment"
        : $"Payment for {car.Year} {car.Model}";

    var gatewayResult = await _paymentGateway.CreatePaymentIntentAsync(paymentIntent.Amount, paymentIntent.Currency, description, cancellationToken).ConfigureAwait(false);
    paymentIntent.ApplyProviderDetails(gatewayResult.ProviderIntentId, gatewayResult.ClientSecret);

    await _paymentIntentRepository.AddAsync(paymentIntent, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return MapToDto(paymentIntent);
  }

  public async Task<PaymentIntentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var paymentIntent = await _paymentIntentRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    return paymentIntent is null ? null : MapToDto(paymentIntent);
  }

  public async Task<IReadOnlyCollection<PaymentIntentDto>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
  {
    var paymentIntents = await _paymentIntentRepository.ListByListingIdAsync(listingId, cancellationToken).ConfigureAwait(false);
    return paymentIntents.Select(MapToDto).OrderByDescending(dto => dto.CreatedAtUtc).ToArray();
  }

  public async Task<PaymentIntentDto> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var paymentIntent = await _paymentIntentRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (paymentIntent is null)
    {
      throw new NotFoundException($"Payment intent '{id}' was not found.");
    }

    if (string.IsNullOrEmpty(paymentIntent.ProviderIntentId))
    {
      throw new ConflictException("Payment intent is missing provider metadata.");
    }

    await _paymentGateway.ConfirmPaymentIntentAsync(paymentIntent.ProviderIntentId, cancellationToken).ConfigureAwait(false);
    paymentIntent.MarkAsSucceeded();

    await _paymentIntentRepository.UpdateAsync(paymentIntent, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return MapToDto(paymentIntent);
  }

  public async Task<PaymentIntentDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var paymentIntent = await _paymentIntentRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (paymentIntent is null)
    {
      throw new NotFoundException($"Payment intent '{id}' was not found.");
    }

    if (string.IsNullOrEmpty(paymentIntent.ProviderIntentId))
    {
      throw new ConflictException("Payment intent is missing provider metadata.");
    }

    await _paymentGateway.CancelPaymentIntentAsync(paymentIntent.ProviderIntentId, cancellationToken).ConfigureAwait(false);
    paymentIntent.Cancel();

    await _paymentIntentRepository.UpdateAsync(paymentIntent, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return MapToDto(paymentIntent);
  }

  private static PaymentIntentDto MapToDto(PaymentIntent paymentIntent)
  {
    return new PaymentIntentDto(
        paymentIntent.Id,
        paymentIntent.ListingId,
        paymentIntent.Amount,
        paymentIntent.Currency,
        paymentIntent.Status,
        paymentIntent.ProviderIntentId,
        paymentIntent.ClientSecret,
        paymentIntent.CreatedAtUtc,
        paymentIntent.UpdatedAtUtc);
  }
}
