using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Recommendations;

public sealed class RecommendationInputModel
{
  [Range(typeof(decimal), "1", "1000000000", ErrorMessage = "Budget must be between 1 and 1,000,000,000.")]
  [Display(Name = "Budget (optional)")]
  public decimal? Budget { get; set; }

  [Range(1886, 2100, ErrorMessage = "Minimum year must be between 1886 and 2100.")]
  [Display(Name = "Minimum Year (optional)")]
  public int? MinYear { get; set; }

  [Range(1, 20, ErrorMessage = "Limit must be between 1 and 20.")]
  public int Limit { get; set; } = 5;
}
