using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Exceptions;
using GenesisCars.Domain.Services;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Tests.Domain.Recommendations;

public class CarRecommendationEngineTests
{
  [Fact]
  public void Recommend_WithBudgetAndMinYear_ReturnsRankedCars()
  {
    var cars = new List<Car>
    {
      Car.Create("Coupe", 2023, 32000m),
      Car.Create("SUV", 2018, 28000m),
      Car.Create("Sedan", 2021, 35000m),
      Car.Create("Roadster", 2016, 45000m)
    };

    var criteria = RecommendationCriteria.Create(33000m, 2018);
    var engine = new CarRecommendationEngine();

    var results = engine.Recommend(cars, criteria, 3);

    Assert.Equal(3, results.Count);
    Assert.All(results, recommendation => Assert.True(recommendation.Car.Year >= 2018));
    var ordered = results.OrderByDescending(r => r.Score).ToArray();
    Assert.Equal(ordered.Select(r => r.Car.Id), results.Select(r => r.Car.Id));
  }

  [Fact]
  public void Recommend_WithLimitGreaterThanInventory_ReturnsAllAvailable()
  {
    var cars = new List<Car>
    {
      Car.Create("Sedan", 2022, 31000m),
      Car.Create("Truck", 2020, 42000m)
    };

    var criteria = RecommendationCriteria.Create(null, null);
    var engine = new CarRecommendationEngine();

    var results = engine.Recommend(cars, criteria, 5);

    Assert.Equal(2, results.Count);
  }

  [Fact]
  public void Recommend_WithNonPositiveLimit_ThrowsDomainException()
  {
    var cars = new List<Car> { Car.Create("Coupe", 2023, 33000m) };
    var criteria = RecommendationCriteria.Create(null, null);
    var engine = new CarRecommendationEngine();

    Assert.Throws<DomainException>(() => engine.Recommend(cars, criteria, 0));
  }
}
