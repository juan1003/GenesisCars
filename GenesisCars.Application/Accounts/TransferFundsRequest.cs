namespace GenesisCars.Application.Accounts;

public sealed record TransferFundsRequest(Guid RecipientAccountId, decimal Amount);
