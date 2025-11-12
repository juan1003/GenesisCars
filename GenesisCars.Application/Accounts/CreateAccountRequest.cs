namespace GenesisCars.Application.Accounts;

public sealed record CreateAccountRequest(string OwnerName, decimal InitialBalance);
