using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Cars;

public class CarInputModel : IValidatableObject
{
  [Required]
  [StringLength(200)]
  public string Model { get; set; } = string.Empty;

  public int Year { get; set; } = DateTime.UtcNow.Year;

  [DataType(DataType.Currency)]
  public decimal Price { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(Model))
    {
      yield return new ValidationResult("Model is required.", new[] { nameof(Model) });
    }
    else if (Model.Trim().Length > 200)
    {
      yield return new ValidationResult("Model cannot be longer than 200 characters.", new[] { nameof(Model) });
    }

    var maxYear = DateTime.UtcNow.Year + 1;
    if (Year < 1886 || Year > maxYear)
    {
      yield return new ValidationResult($"Year must be between 1886 and {maxYear}.", new[] { nameof(Year) });
    }

    if (Price <= 0)
    {
      yield return new ValidationResult("Price must be greater than zero.", new[] { nameof(Price) });
    }
    else if (Price > 1_000_000_000m)
    {
      yield return new ValidationResult("Price must be less than or equal to 1,000,000,000.", new[] { nameof(Price) });
    }
  }
}
