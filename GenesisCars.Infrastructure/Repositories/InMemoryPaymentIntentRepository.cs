using System.Linq;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Infrastructure.Repositories;

public sealed class InMemoryPaymentIntentRepository : IPaymentIntentRepository
{
  private readonly List<PaymentIntent> _paymentIntents = new();
  private readonly SemaphoreSlim _lock = new(1, 1);

  public async Task AddAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      _paymentIntents.Add(paymentIntent);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task UpdateAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      var index = _paymentIntents.FindIndex(existing => existing.Id == paymentIntent.Id);
      if (index >= 0)
      {
        _paymentIntents[index] = paymentIntent;
      }
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<PaymentIntent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _paymentIntents.FirstOrDefault(existing => existing.Id == id);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<IReadOnlyList<PaymentIntent>> ListByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _paymentIntents
          .Where(existing => existing.ListingId == listingId)
          .OrderByDescending(existing => existing.CreatedAtUtc)
          .ToList();
    }
    finally
    {
      _lock.Release();
    }
  }
}
