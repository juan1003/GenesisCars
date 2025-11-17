namespace GenesisCars.Application.Marketplace;

public sealed record CreateMarketplaceListingRequest(
    Guid CarId,
    decimal AskingPrice,
    string? Description
);
