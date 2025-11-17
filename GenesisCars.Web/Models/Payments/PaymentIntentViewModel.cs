using GenesisCars.Application.Marketplace;
using GenesisCars.Application.Payments;

namespace GenesisCars.Web.Models.Payments;

public sealed class PaymentIntentViewModel
{
  public required MarketplaceListingDto Listing { get; init; }

  public required PaymentIntentDto PaymentIntent { get; init; }
}
