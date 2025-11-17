namespace GenesisCars.Application.Payments;

public sealed record CreatePaymentIntentRequest(
    Guid ListingId,
    string Currency
);
