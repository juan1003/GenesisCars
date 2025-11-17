using GenesisCars.Domain.Entities;

namespace GenesisCars.Application.Marketplace;

public sealed record MarketplaceListingDto(
    Guid Id,
    MarketplaceListingCarDto Car,
    decimal AskingPrice,
    MarketplaceListingStatus Status,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);
