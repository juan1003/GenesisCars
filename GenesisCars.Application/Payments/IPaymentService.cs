namespace GenesisCars.Application.Payments;

public interface IPaymentService
{
  Task<PaymentIntentDto> CreateAsync(CreatePaymentIntentRequest request, CancellationToken cancellationToken = default);

  Task<PaymentIntentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<PaymentIntentDto>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default);

  Task<PaymentIntentDto> ConfirmAsync(Guid id, CancellationToken cancellationToken = default);

  Task<PaymentIntentDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
