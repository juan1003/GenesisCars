using GenesisCars.Domain.Exceptions;

namespace GenesisCars.Domain.Entities;

public sealed class PaymentIntent
{
  private PaymentIntent()
  {
  }

  private PaymentIntent(Guid id, Guid listingId, decimal amount, string currency)
  {
    if (listingId == Guid.Empty)
    {
      throw new DomainException("ListingId is required for a payment intent.");
    }

    Id = id;
    ListingId = listingId;
    Amount = ValidateAmount(amount);
    Currency = NormalizeCurrency(currency);
    Status = PaymentStatus.Pending;
    CreatedAtUtc = DateTime.UtcNow;
    UpdatedAtUtc = CreatedAtUtc;
  }

  public Guid Id { get; private set; }

  public Guid ListingId { get; private set; }

  public decimal Amount { get; private set; }

  public string Currency { get; private set; } = string.Empty;

  public PaymentStatus Status { get; private set; }

  public string? ProviderIntentId { get; private set; }

  public string? ClientSecret { get; private set; }

  public DateTime CreatedAtUtc { get; private set; }

  public DateTime UpdatedAtUtc { get; private set; }

  public static PaymentIntent Create(Guid listingId, decimal amount, string currency)
  {
    return new PaymentIntent(Guid.NewGuid(), listingId, amount, currency);
  }

  public void ApplyProviderDetails(string providerIntentId, string clientSecret)
  {
    if (string.IsNullOrWhiteSpace(providerIntentId))
    {
      throw new DomainException("Provider intent id is required.");
    }

    if (string.IsNullOrWhiteSpace(clientSecret))
    {
      throw new DomainException("Client secret is required.");
    }

    ProviderIntentId = providerIntentId.Trim();
    ClientSecret = clientSecret.Trim();
    Touch();
  }

  public void MarkAsSucceeded()
  {
    if (Status == PaymentStatus.Canceled)
    {
      throw new DomainException("Canceled payment intents cannot be marked as succeeded.");
    }

    if (Status == PaymentStatus.Succeeded)
    {
      return;
    }

    Status = PaymentStatus.Succeeded;
    Touch();
  }

  public void Cancel()
  {
    if (Status == PaymentStatus.Succeeded)
    {
      throw new DomainException("Succeeded payment intents cannot be canceled.");
    }

    if (Status == PaymentStatus.Canceled)
    {
      return;
    }

    Status = PaymentStatus.Canceled;
    Touch();
  }

  private static decimal ValidateAmount(decimal amount)
  {
    if (amount <= 0)
    {
      throw new DomainException("Payment amount must be greater than zero.");
    }

    if (amount > 1_000_000_000m)
    {
      throw new DomainException("Payment amount must be less than or equal to 1,000,000,000.");
    }

    return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
  }

  private static string NormalizeCurrency(string currency)
  {
    if (string.IsNullOrWhiteSpace(currency))
    {
      throw new DomainException("Currency is required.");
    }

    var normalized = currency.Trim().ToUpperInvariant();
    if (normalized.Length is < 3 or > 6)
    {
      throw new DomainException("Currency must be between 3 and 6 characters.");
    }

    return normalized;
  }

  private void Touch()
  {
    UpdatedAtUtc = DateTime.UtcNow;
  }
}
