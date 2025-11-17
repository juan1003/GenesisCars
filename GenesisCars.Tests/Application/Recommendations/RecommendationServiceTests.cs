using GenesisCars.Application.Recommendations;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Exceptions;
using GenesisCars.Domain.Repositories;
using GenesisCars.Domain.Services;

namespace GenesisCars.Tests.Application.Recommendations;

public class RecommendationServiceTests
{
  [Fact]
  public async Task GetRecommendationsAsync_ExcludesActiveListings()
  {
    var availableCar = Car.Create("SUV", 2022, 28000m);
    var listedCar = Car.Create("Coupe", 2023, 32000m);

    var carRepository = new TestCarRepository(availableCar, listedCar);
    var activeListing = MarketplaceListing.Create(listedCar.Id, listedCar.Price, null);
    var listingRepository = new TestMarketplaceListingRepository(activeListing);
    var engine = new CarRecommendationEngine();

    var service = new RecommendationService(carRepository, listingRepository, engine);

    var request = new RecommendationRequest(30000m, 2020, 5);
    var results = await service.GetRecommendationsAsync(request);

    Assert.Single(results);
    Assert.DoesNotContain(results, dto => dto.CarId == listedCar.Id);
    Assert.Equal(availableCar.Id, results.Single().CarId);
  }

  [Fact]
  public async Task GetRecommendationsAsync_WithLimitGreaterThanMax_IsClamped()
  {
    var cars = Enumerable.Range(0, 25)
        .Select(index => Car.Create($"Model {index}", 2021 - (index % 3), 25000m + index * 500))
        .ToArray();

    var carRepository = new TestCarRepository(cars);
    var listingRepository = new TestMarketplaceListingRepository();
    var service = new RecommendationService(carRepository, listingRepository, new CarRecommendationEngine());

    var request = new RecommendationRequest(null, null, 50);
    var results = await service.GetRecommendationsAsync(request);

    Assert.InRange(results.Count, 1, 20);
  }

  [Fact]
  public async Task GetRecommendationsAsync_WithInvalidBudget_ThrowsDomainException()
  {
    var carRepository = new TestCarRepository(Car.Create("Sedan", 2022, 30000m));
    var listingRepository = new TestMarketplaceListingRepository();
    var service = new RecommendationService(carRepository, listingRepository, new CarRecommendationEngine());

    var request = new RecommendationRequest(-1000m, null, 5);

    await Assert.ThrowsAsync<DomainException>(() => service.GetRecommendationsAsync(request));
  }

  private sealed class TestCarRepository : ICarRepository
  {
    private readonly List<Car> _items;

    public TestCarRepository(params Car[] cars)
    {
      _items = cars.ToList();
    }

    public Task AddAsync(Car car, CancellationToken cancellationToken = default)
    {
      _items.Add(car);
      return Task.CompletedTask;
    }

    public Task DeleteAsync(Car car, CancellationToken cancellationToken = default)
    {
      _items.RemoveAll(existing => existing.Id == car.Id);
      return Task.CompletedTask;
    }

    public Task<Car?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      return Task.FromResult<Car?>(_items.FirstOrDefault(car => car.Id == id));
    }

    public Task<IReadOnlyList<Car>> ListAsync(CancellationToken cancellationToken = default)
    {
      IReadOnlyList<Car> snapshot = _items.ToList();
      return Task.FromResult(snapshot);
    }

    public Task UpdateAsync(Car car, CancellationToken cancellationToken = default)
    {
      var index = _items.FindIndex(existing => existing.Id == car.Id);
      if (index >= 0)
      {
        _items[index] = car;
      }

      return Task.CompletedTask;
    }
  }

  private sealed class TestMarketplaceListingRepository : IMarketplaceListingRepository
  {
    private readonly List<MarketplaceListing> _items;

    public TestMarketplaceListingRepository(params MarketplaceListing[] listings)
    {
      _items = listings.ToList();
    }

    public Task AddAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
    {
      _items.Add(listing);
      return Task.CompletedTask;
    }

    public Task DeleteAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
    {
      _items.RemoveAll(existing => existing.Id == listing.Id);
      return Task.CompletedTask;
    }

    public Task<MarketplaceListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      return Task.FromResult<MarketplaceListing?>(_items.FirstOrDefault(listing => listing.Id == id));
    }

    public Task<IReadOnlyList<MarketplaceListing>> ListAsync(CancellationToken cancellationToken = default)
    {
      IReadOnlyList<MarketplaceListing> snapshot = _items.ToList();
      return Task.FromResult(snapshot);
    }

    public Task<IReadOnlyList<MarketplaceListing>> ListByCarIdAsync(Guid carId, CancellationToken cancellationToken = default)
    {
      IReadOnlyList<MarketplaceListing> snapshot = _items.Where(listing => listing.CarId == carId).ToList();
      return Task.FromResult(snapshot);
    }

    public Task UpdateAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
    {
      var index = _items.FindIndex(existing => existing.Id == listing.Id);
      if (index >= 0)
      {
        _items[index] = listing;
      }

      return Task.CompletedTask;
    }
  }
}
