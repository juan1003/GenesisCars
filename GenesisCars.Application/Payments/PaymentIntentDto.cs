using GenesisCars.Domain.Entities;

namespace GenesisCars.Application.Payments;

public sealed record PaymentIntentDto(
    Guid Id,
    Guid ListingId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    string? ProviderIntentId,
    string? ClientSecret,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);
