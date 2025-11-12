namespace GenesisCars.Domain.Entities;

public sealed record AccountTransaction(
    Guid Id,
    DateTime TimestampUtc,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string? Description
);
