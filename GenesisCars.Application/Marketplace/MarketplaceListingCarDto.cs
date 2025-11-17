namespace GenesisCars.Application.Marketplace;

public sealed record MarketplaceListingCarDto(
    Guid Id,
    string Model,
    int Year,
    decimal InventoryPrice
);
