using GenesisCars.Domain.Entities;

namespace GenesisCars.Domain.Repositories;

public interface IMarketplaceListingRepository
{
  Task AddAsync(MarketplaceListing listing, CancellationToken cancellationToken = default);

  Task UpdateAsync(MarketplaceListing listing, CancellationToken cancellationToken = default);

  Task DeleteAsync(MarketplaceListing listing, CancellationToken cancellationToken = default);

  Task<MarketplaceListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  Task<IReadOnlyList<MarketplaceListing>> ListAsync(CancellationToken cancellationToken = default);

  Task<IReadOnlyList<MarketplaceListing>> ListByCarIdAsync(Guid carId, CancellationToken cancellationToken = default);
}
