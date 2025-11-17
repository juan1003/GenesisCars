namespace GenesisCars.Application.Marketplace;

public sealed record UpdateMarketplaceListingRequest(
    decimal AskingPrice,
    string? Description
);
