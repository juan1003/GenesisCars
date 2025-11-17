using GenesisCars.Domain.Exceptions;

namespace GenesisCars.Domain.ValueObjects;

public sealed class RecommendationCriteria
{
  private RecommendationCriteria(decimal? budget, int? minYear)
  {
    Budget = budget;
    MinYear = minYear;
  }

  public decimal? Budget { get; }

  public int? MinYear { get; }

  public static RecommendationCriteria Create(decimal? budget, int? minYear)
  {
    if (budget.HasValue)
    {
      if (budget.Value <= 0)
      {
        throw new DomainException("Budget must be greater than zero when provided.");
      }

      if (budget.Value > 1_000_000_000m)
      {
        throw new DomainException("Budget must be less than or equal to 1,000,000,000.");
      }
    }

    if (minYear.HasValue)
    {
      var currentYear = DateTime.UtcNow.Year + 1;
      if (minYear.Value < 1886 || minYear.Value > currentYear)
      {
        throw new DomainException($"Minimum year must be between 1886 and {currentYear}.");
      }
    }

    return new RecommendationCriteria(budget, minYear);
  }
}
