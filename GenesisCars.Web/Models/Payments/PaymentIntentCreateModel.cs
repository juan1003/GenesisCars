using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Payments;

public sealed class PaymentIntentCreateModel
{
  [Required]
  public Guid ListingId { get; set; }

  [Required]
  [StringLength(6, MinimumLength = 3)]
  public string Currency { get; set; } = "USD";
}
