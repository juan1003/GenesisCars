using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Marketplace;

public sealed class MarketplaceListingInputModel
{
  [Display(Name = "Car")]
  [Required]
  public Guid CarId { get; set; }

  [Display(Name = "Asking Price")]
  [Range(0.01, 1_000_000_000)]
  public decimal AskingPrice { get; set; }

  [Display(Name = "Description")]
  [StringLength(1000)]
  public string? Description { get; set; }
}
