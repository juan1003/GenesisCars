using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Marketplace;

public sealed class MarketplaceListingEditModel
{
  [Display(Name = "Asking Price")]
  [Range(0.01, 1_000_000_000)]
  public decimal AskingPrice { get; set; }

  [Display(Name = "Description")]
  [StringLength(1000)]
  public string? Description { get; set; }
}
