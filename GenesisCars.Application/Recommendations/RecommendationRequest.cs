namespace GenesisCars.Application.Recommendations;

public sealed class RecommendationRequest
{
  public RecommendationRequest(decimal? budget, int? minYear, int limit = 5)
  {
    Budget = budget;
    MinYear = minYear;
    Limit = limit;
  }

  public decimal? Budget { get; }

  public int? MinYear { get; }

  public int Limit { get; }
}
