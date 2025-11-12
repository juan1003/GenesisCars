namespace GenesisCars.Application.Accounts;

public sealed record AccountDto(
    Guid Id,
    string OwnerName,
    decimal Balance,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<AccountTransactionDto> Transactions
);
