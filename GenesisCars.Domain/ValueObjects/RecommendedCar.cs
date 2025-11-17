using GenesisCars.Domain.Entities;

namespace GenesisCars.Domain.ValueObjects;

public sealed class RecommendedCar
{
  public RecommendedCar(Car car, decimal score)
  {
    Car = car;
    Score = decimal.Round(score, 2, MidpointRounding.AwayFromZero);
  }

  public Car Car { get; }

  public decimal Score { get; }
}
