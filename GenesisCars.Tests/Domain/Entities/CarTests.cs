using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Exceptions;

namespace GenesisCars.Tests.Domain.Entities;

public class CarTests
{
  [Fact]
  public void Create_WithValidValues_ReturnsCar()
  {
    var car = Car.Create("Model S", 2024, 79999.99m);

    Assert.Equal("Model S", car.Model);
    Assert.Equal(2024, car.Year);
    Assert.Equal(79999.99m, car.Price);
    Assert.NotEqual(Guid.Empty, car.Id);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Create_WithInvalidModel_ThrowsDomainException(string model)
  {
    Assert.Throws<DomainException>(() => Car.Create(model, 2024, 10000m));
  }

  [Theory]
  [InlineData(1885)]
  [InlineData(3000)]
  public void Create_WithInvalidYear_ThrowsDomainException(int year)
  {
    Assert.Throws<DomainException>(() => Car.Create("Model X", year, 10000m));
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-100)]
  public void Create_WithInvalidPrice_ThrowsDomainException(decimal price)
  {
    Assert.Throws<DomainException>(() => Car.Create("Model X", 2024, price));
  }

  [Fact]
  public void Update_UpdatesCarDetails()
  {
    var car = Car.Create("Model 3", 2023, 35000m);

    car.Update("Model 3 Performance", 2024, 55000.123m);

    Assert.Equal("Model 3 Performance", car.Model);
    Assert.Equal(2024, car.Year);
    Assert.Equal(55000.12m, car.Price);
  }
}
