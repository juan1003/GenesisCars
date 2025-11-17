namespace GenesisCars.Application.Marketplace;

public interface IMarketplaceService
{
  Task<IReadOnlyCollection<MarketplaceListingDto>> GetAllAsync(CancellationToken cancellationToken = default);

  Task<MarketplaceListingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  Task<MarketplaceListingDto> CreateAsync(CreateMarketplaceListingRequest request, CancellationToken cancellationToken = default);

  Task<MarketplaceListingDto> UpdateAsync(Guid id, UpdateMarketplaceListingRequest request, CancellationToken cancellationToken = default);

  Task<MarketplaceListingDto> MarkAsSoldAsync(Guid id, CancellationToken cancellationToken = default);

  Task<MarketplaceListingDto> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
