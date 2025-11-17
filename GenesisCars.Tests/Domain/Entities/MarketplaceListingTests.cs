using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Exceptions;

namespace GenesisCars.Tests.Domain.Entities;

public class MarketplaceListingTests
{
  [Fact]
  public void Create_WithValidValues_SetsInitialState()
  {
    var carId = Guid.NewGuid();
    var listing = MarketplaceListing.Create(carId, 25000m, "Low mileage");

    Assert.Equal(carId, listing.CarId);
    Assert.Equal(25000m, listing.AskingPrice);
    Assert.Equal("Low mileage", listing.Description);
    Assert.Equal(MarketplaceListingStatus.Active, listing.Status);
    Assert.True((DateTime.UtcNow - listing.CreatedAtUtc).TotalSeconds < 5);
    Assert.True((DateTime.UtcNow - listing.UpdatedAtUtc).TotalSeconds < 5);
  }

  [Fact]
  public void UpdateAskingPrice_WithInvalidValue_Throws()
  {
    var listing = MarketplaceListing.Create(Guid.NewGuid(), 10000m, null);

    Assert.Throws<DomainException>(() => listing.UpdateAskingPrice(0));
  }

  [Fact]
  public void MarkAsSold_FromActive_SetsSoldStatus()
  {
    var listing = MarketplaceListing.Create(Guid.NewGuid(), 32000m, null);

    listing.MarkAsSold();

    Assert.Equal(MarketplaceListingStatus.Sold, listing.Status);
  }

  [Fact]
  public void MarkAsSold_WhenArchived_Throws()
  {
    var listing = MarketplaceListing.Create(Guid.NewGuid(), 32000m, null);
    listing.Archive();

    Assert.Throws<DomainException>(listing.MarkAsSold);
  }

  [Fact]
  public void UpdateDescription_WhenArchived_Throws()
  {
    var listing = MarketplaceListing.Create(Guid.NewGuid(), 15000m, "Clean title");
    listing.Archive();

    Assert.Throws<DomainException>(() => listing.UpdateDescription("Updated"));
  }
}
