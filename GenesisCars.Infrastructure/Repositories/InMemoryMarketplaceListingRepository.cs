using System.Linq;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Infrastructure.Repositories;

public sealed class InMemoryMarketplaceListingRepository : IMarketplaceListingRepository
{
  private readonly List<MarketplaceListing> _listings = new();
  private readonly SemaphoreSlim _lock = new(1, 1);

  public async Task AddAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      _listings.Add(listing);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task DeleteAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      _listings.RemoveAll(existing => existing.Id == listing.Id);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<MarketplaceListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _listings.FirstOrDefault(listing => listing.Id == id);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<IReadOnlyList<MarketplaceListing>> ListAsync(CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _listings
          .OrderByDescending(listing => listing.CreatedAtUtc)
          .ToList();
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<IReadOnlyList<MarketplaceListing>> ListByCarIdAsync(Guid carId, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _listings
          .Where(listing => listing.CarId == carId)
          .ToList();
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task UpdateAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      var index = _listings.FindIndex(existing => existing.Id == listing.Id);
      if (index >= 0)
      {
        _listings[index] = listing;
      }
    }
    finally
    {
      _lock.Release();
    }
  }
}
