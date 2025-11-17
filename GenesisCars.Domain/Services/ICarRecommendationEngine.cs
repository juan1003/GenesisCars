using GenesisCars.Domain.Entities;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Domain.Services;

public interface ICarRecommendationEngine
{
  IReadOnlyCollection<RecommendedCar> Recommend(
      IReadOnlyCollection<Car> cars,
      RecommendationCriteria criteria,
      int limit);
}
