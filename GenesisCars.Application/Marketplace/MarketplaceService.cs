using System.Linq;
using GenesisCars.Application.Exceptions;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Application.Marketplace;

public sealed class MarketplaceService : IMarketplaceService
{
  private readonly IMarketplaceListingRepository _listingRepository;
  private readonly ICarRepository _carRepository;
  private readonly IUnitOfWork _unitOfWork;

  public MarketplaceService(
      IMarketplaceListingRepository listingRepository,
      ICarRepository carRepository,
      IUnitOfWork unitOfWork)
  {
    _listingRepository = listingRepository;
    _carRepository = carRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IReadOnlyCollection<MarketplaceListingDto>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    var listings = await _listingRepository.ListAsync(cancellationToken).ConfigureAwait(false);
    var cars = await _carRepository.ListAsync(cancellationToken).ConfigureAwait(false);
    var carLookup = cars.ToDictionary(car => car.Id, car => car);

    return listings
        .Select(listing => MapToDto(listing, carLookup.TryGetValue(listing.CarId, out var car) ? car : null))
        .ToArray();
  }

  public async Task<MarketplaceListingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var listing = await _listingRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (listing is null)
    {
      return null;
    }

    var car = await _carRepository.GetByIdAsync(listing.CarId, cancellationToken).ConfigureAwait(false);
    return MapToDto(listing, car);
  }

  public async Task<MarketplaceListingDto> CreateAsync(CreateMarketplaceListingRequest request, CancellationToken cancellationToken = default)
  {
    var car = await _carRepository.GetByIdAsync(request.CarId, cancellationToken).ConfigureAwait(false);
    if (car is null)
    {
      throw new NotFoundException($"Car '{request.CarId}' was not found.");
    }

    var existingListings = await _listingRepository.ListByCarIdAsync(request.CarId, cancellationToken).ConfigureAwait(false);
    if (existingListings.Any(listing => listing.Status == MarketplaceListingStatus.Active))
    {
      throw new ConflictException($"Car '{car.Model}' already has an active listing.");
    }

    var listing = MarketplaceListing.Create(request.CarId, request.AskingPrice, request.Description);
    await _listingRepository.AddAsync(listing, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return MapToDto(listing, car);
  }

  public async Task<MarketplaceListingDto> UpdateAsync(Guid id, UpdateMarketplaceListingRequest request, CancellationToken cancellationToken = default)
  {
    var listing = await _listingRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (listing is null)
    {
      throw new NotFoundException($"Listing '{id}' was not found.");
    }

    listing.UpdateAskingPrice(request.AskingPrice);
    listing.UpdateDescription(request.Description);

    await _listingRepository.UpdateAsync(listing, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    var car = await _carRepository.GetByIdAsync(listing.CarId, cancellationToken).ConfigureAwait(false);
    return MapToDto(listing, car);
  }

  public async Task<MarketplaceListingDto> MarkAsSoldAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var listing = await _listingRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (listing is null)
    {
      throw new NotFoundException($"Listing '{id}' was not found.");
    }

    listing.MarkAsSold();

    await _listingRepository.UpdateAsync(listing, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    var car = await _carRepository.GetByIdAsync(listing.CarId, cancellationToken).ConfigureAwait(false);
    return MapToDto(listing, car);
  }

  public async Task<MarketplaceListingDto> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var listing = await _listingRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (listing is null)
    {
      throw new NotFoundException($"Listing '{id}' was not found.");
    }

    listing.Archive();

    await _listingRepository.UpdateAsync(listing, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    var car = await _carRepository.GetByIdAsync(listing.CarId, cancellationToken).ConfigureAwait(false);
    return MapToDto(listing, car);
  }

  private static MarketplaceListingDto MapToDto(MarketplaceListing listing, Car? car)
  {
    var carDto = car is null
        ? new MarketplaceListingCarDto(listing.CarId, "Unknown", 0, 0m)
        : new MarketplaceListingCarDto(car.Id, car.Model, car.Year, car.Price);

    return new MarketplaceListingDto(
        listing.Id,
        carDto,
        listing.AskingPrice,
        listing.Status,
        listing.Description,
        listing.CreatedAtUtc,
        listing.UpdatedAtUtc);
  }
}
