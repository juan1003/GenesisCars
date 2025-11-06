using GenesisCars.Domain.Exceptions;

namespace GenesisCars.Domain.Entities;

public class Car
{
  private Car()
  {
  }

  private Car(Guid id, string model, int year, decimal price)
  {
    Id = id;
    UpdateDetails(model, year, price);
    CreatedAtUtc = DateTime.UtcNow;
    UpdatedAtUtc = CreatedAtUtc;
  }

  public Guid Id { get; private set; }

  public string Model { get; private set; } = string.Empty;

  public int Year { get; private set; }

  public decimal Price { get; private set; }

  public DateTime CreatedAtUtc { get; private set; }

  public DateTime UpdatedAtUtc { get; private set; }

  public static Car Create(string model, int year, decimal price)
  {
    return new Car(Guid.NewGuid(), model, year, price);
  }

  public void Update(string model, int year, decimal price)
  {
    UpdateDetails(model, year, price);
    UpdatedAtUtc = DateTime.UtcNow;
  }

  private void UpdateDetails(string model, int year, decimal price)
  {
    if (string.IsNullOrWhiteSpace(model))
    {
      throw new DomainException("Model is required.");
    }

    var trimmedModel = model.Trim();
    if (trimmedModel.Length > 200)
    {
      throw new DomainException("Model cannot be longer than 200 characters.");
    }

    var currentYear = DateTime.UtcNow.Year + 1;
    if (year < 1886 || year > currentYear)
    {
      throw new DomainException($"Year must be between 1886 and {currentYear}.");
    }

    if (price <= 0)
    {
      throw new DomainException("Price must be greater than zero.");
    }

    if (price > 1_000_000_000m)
    {
      throw new DomainException("Price must be less than or equal to 1,000,000,000.");
    }

    Model = trimmedModel;
    Year = year;
    Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
  }
}
