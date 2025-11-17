using GenesisCars.Domain.Entities;

namespace GenesisCars.Domain.Repositories;

public interface IPaymentIntentRepository
{
  Task AddAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken = default);

  Task UpdateAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken = default);

  Task<PaymentIntent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  Task<IReadOnlyList<PaymentIntent>> ListByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default);
}
