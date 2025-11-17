using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Exceptions;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Domain.Services;

public sealed class CarRecommendationEngine : ICarRecommendationEngine
{
  private const decimal BudgetWeight = 60m;
  private const decimal RecencyWeight = 30m;
  private const decimal AvailabilityWeight = 10m;

  public IReadOnlyCollection<RecommendedCar> Recommend(
      IReadOnlyCollection<Car> cars,
      RecommendationCriteria criteria,
      int limit)
  {
    if (cars is null)
    {
      throw new ArgumentNullException(nameof(cars));
    }

    if (criteria is null)
    {
      throw new ArgumentNullException(nameof(criteria));
    }

    if (limit <= 0)
    {
      throw new DomainException("Recommendation limit must be greater than zero.");
    }

    if (cars.Count == 0)
    {
      return Array.Empty<RecommendedCar>();
    }

    var currentYear = DateTime.UtcNow.Year;
    var results = cars
        .Where(car => criteria.MinYear is null || car.Year >= criteria.MinYear.Value)
        .Select(car => new RecommendedCar(car, CalculateScore(car, criteria, currentYear)))
        .OrderByDescending(result => result.Score)
        .ThenBy(result => result.Car.Price)
        .Take(limit)
        .ToArray();

    return results;
  }

  private static decimal CalculateScore(Car car, RecommendationCriteria criteria, int currentYear)
  {
    var score = AvailabilityWeight; // baseline to avoid zeroed scores.

    if (criteria.Budget.HasValue)
    {
      var budget = criteria.Budget.Value;
      var difference = Math.Abs(car.Price - budget);
      if (budget > 0)
      {
        var ratio = difference / budget;
        var clampedRatio = Math.Min(1m, ratio);
        var budgetComponent = BudgetWeight * (1m - clampedRatio);
        if (car.Price <= budget)
        {
          budgetComponent += 5m;
        }

        score += Math.Max(0m, budgetComponent);
      }
    }
    else
    {
      score += BudgetWeight / 2m;
    }

    var age = Math.Max(0, currentYear - car.Year);
    var recencyComponent = Math.Max(0m, RecencyWeight - (age * 3m));
    score += recencyComponent;

    return Math.Min(100m, score);
  }
}
