namespace GenesisCars.Application.Payments;

public sealed record PaymentGatewayCreateResult(
    string ProviderIntentId,
    string ClientSecret
);
