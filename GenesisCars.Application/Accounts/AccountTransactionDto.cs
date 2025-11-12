namespace GenesisCars.Application.Accounts;

public sealed record AccountTransactionDto(
    Guid Id,
    DateTime TimestampUtc,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string? Description
);
