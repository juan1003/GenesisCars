using GenesisCars.Domain.Exceptions;

namespace GenesisCars.Domain.Entities;

public sealed class MarketplaceListing
{
  private MarketplaceListing()
  {
  }

  private MarketplaceListing(Guid id, Guid carId, decimal askingPrice, string? description)
  {
    if (carId == Guid.Empty)
    {
      throw new DomainException("CarId is required to create a listing.");
    }

    Id = id;
    CarId = carId;
    Status = MarketplaceListingStatus.Active;
    CreatedAtUtc = DateTime.UtcNow;
    UpdatedAtUtc = CreatedAtUtc;

    AskingPrice = ValidatePrice(askingPrice);
    Description = NormalizeDescription(description);
  }

  public Guid Id { get; private set; }

  public Guid CarId { get; private set; }

  public decimal AskingPrice { get; private set; }

  public string? Description { get; private set; }

  public MarketplaceListingStatus Status { get; private set; }

  public DateTime CreatedAtUtc { get; private set; }

  public DateTime UpdatedAtUtc { get; private set; }

  public static MarketplaceListing Create(Guid carId, decimal askingPrice, string? description)
  {
    return new MarketplaceListing(Guid.NewGuid(), carId, askingPrice, description);
  }

  public void UpdateAskingPrice(decimal askingPrice)
  {
    EnsureNotArchived();
    AskingPrice = ValidatePrice(askingPrice);
    Touch();
  }

  public void UpdateDescription(string? description)
  {
    EnsureNotArchived();
    Description = NormalizeDescription(description);
    Touch();
  }

  public void MarkAsSold()
  {
    if (Status == MarketplaceListingStatus.Archived)
    {
      throw new DomainException("Archived listings cannot be marked as sold.");
    }

    if (Status == MarketplaceListingStatus.Sold)
    {
      return;
    }

    if (Status != MarketplaceListingStatus.Active)
    {
      throw new DomainException("Only active listings can be marked as sold.");
    }

    Status = MarketplaceListingStatus.Sold;
    Touch();
  }

  public void Archive()
  {
    if (Status == MarketplaceListingStatus.Archived)
    {
      return;
    }

    Status = MarketplaceListingStatus.Archived;
    Touch();
  }

  public void Activate()
  {
    if (Status == MarketplaceListingStatus.Archived)
    {
      throw new DomainException("Archived listings cannot be reactivated.");
    }

    if (Status == MarketplaceListingStatus.Active)
    {
      return;
    }

    Status = MarketplaceListingStatus.Active;
    Touch();
  }

  private static decimal ValidatePrice(decimal price)
  {
    if (price <= 0)
    {
      throw new DomainException("Asking price must be greater than zero.");
    }

    if (price > 1_000_000_000m)
    {
      throw new DomainException("Asking price must be less than or equal to 1,000,000,000.");
    }

    return decimal.Round(price, 2, MidpointRounding.AwayFromZero);
  }

  private static string? NormalizeDescription(string? description)
  {
    if (string.IsNullOrWhiteSpace(description))
    {
      return null;
    }

    var trimmed = description.Trim();
    if (trimmed.Length > 1000)
    {
      throw new DomainException("Description cannot exceed 1,000 characters.");
    }

    return trimmed;
  }

  private void EnsureNotArchived()
  {
    if (Status == MarketplaceListingStatus.Archived)
    {
      throw new DomainException("Archived listings cannot be modified.");
    }
  }

  private void Touch()
  {
    UpdatedAtUtc = DateTime.UtcNow;
  }
}
