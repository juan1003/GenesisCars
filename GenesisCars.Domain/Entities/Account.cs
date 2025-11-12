using GenesisCars.Domain.Exceptions;

namespace GenesisCars.Domain.Entities;

public sealed class Account
{
  private readonly List<AccountTransaction> _transactions = new();

  private Account() { }

  private Account(Guid id, string ownerName, decimal balance)
  {
    Id = id;
    OwnerName = ownerName?.Trim() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(OwnerName))
    {
      throw new DomainException("Owner name is required.");
    }

    if (balance < 0m)
    {
      throw new DomainException("Starting balance cannot be negative.");
    }

    Balance = decimal.Round(balance, 2, MidpointRounding.AwayFromZero);
    CreatedAtUtc = DateTime.UtcNow;
    UpdatedAtUtc = CreatedAtUtc;

    RecordTransaction("AccountOpened", 0m, Balance, "Account created");

    if (Balance > 0m)
    {
      RecordTransaction("Credit", Balance, Balance, "Initial deposit");
    }
  }

  public Guid Id { get; private set; }

  public string OwnerName { get; private set; } = string.Empty;

  public decimal Balance { get; private set; }

  public DateTime CreatedAtUtc { get; private set; }

  public DateTime UpdatedAtUtc { get; private set; }

  public IReadOnlyCollection<AccountTransaction> Transactions => _transactions.AsReadOnly();

  public static Account Create(string ownerName, decimal initialBalance = 0m)
  {
    return new Account(Guid.NewGuid(), ownerName, initialBalance);
  }

  public void Credit(decimal amount, string? description = null)
  {
    if (amount <= 0m)
    {
      throw new DomainException("Credit amount must be greater than zero.");
    }

    Balance = decimal.Round(Balance + amount, 2, MidpointRounding.AwayFromZero);
    UpdatedAtUtc = DateTime.UtcNow;

    RecordTransaction("Credit", amount, Balance, description);
  }

  public void Debit(decimal amount, string? description = null)
  {
    if (amount <= 0m)
    {
      throw new DomainException("Debit amount must be greater than zero.");
    }

    if (amount > Balance)
    {
      throw new DomainException("Insufficient funds.");
    }

    Balance = decimal.Round(Balance - amount, 2, MidpointRounding.AwayFromZero);
    UpdatedAtUtc = DateTime.UtcNow;

    RecordTransaction("Debit", amount, Balance, description);
  }

  public void TransferTo(Account recipient, decimal amount)
  {
    if (recipient == null) throw new ArgumentNullException(nameof(recipient));
    if (recipient.Id == this.Id) throw new DomainException("Cannot transfer to the same account.");

    // Perform debit first to ensure atomic behavior in memory
    Debit(amount, $"Transfer to {recipient.OwnerName}");
    recipient.Credit(amount, $"Transfer from {OwnerName}");
  }

  private void RecordTransaction(string type, decimal amount, decimal balanceAfter, string? description)
  {
    var sanitizedAmount = decimal.Round(Math.Abs(amount), 2, MidpointRounding.AwayFromZero);
    _transactions.Add(new AccountTransaction(
        Guid.NewGuid(),
        DateTime.UtcNow,
        type,
        sanitizedAmount,
        decimal.Round(balanceAfter, 2, MidpointRounding.AwayFromZero),
        string.IsNullOrWhiteSpace(description) ? null : description.Trim()));
  }
}
