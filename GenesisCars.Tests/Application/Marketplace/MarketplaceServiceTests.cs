using System.Linq;
using GenesisCars.Application.Exceptions;
using GenesisCars.Application.Marketplace;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Tests.Application.Marketplace;

public class MarketplaceServiceTests
{
  [Fact]
  public async Task CreateAsync_WithValidRequest_PersistsListing()
  {
    var car = Car.Create("Roadster", 2024, 95000m);
    var carRepository = new TestCarRepository(car);
    var listingRepository = new TestMarketplaceListingRepository();
    var service = new MarketplaceService(listingRepository, carRepository, new TrackingUnitOfWork());

    var result = await service.CreateAsync(new CreateMarketplaceListingRequest(car.Id, 99000m, "Limited edition"));

    Assert.Single(listingRepository.Listings);
    Assert.Equal(MarketplaceListingStatus.Active, result.Status);
    Assert.Equal("Roadster", result.Car.Model);
  }

  [Fact]
  public async Task CreateAsync_WithMissingCar_ThrowsNotFound()
  {
    var carRepository = new TestCarRepository();
    var listingRepository = new TestMarketplaceListingRepository();
    var service = new MarketplaceService(listingRepository, carRepository, new TrackingUnitOfWork());

    await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(new CreateMarketplaceListingRequest(Guid.NewGuid(), 50000m, null)));
  }

  [Fact]
  public async Task CreateAsync_WithExistingActiveListing_ThrowsConflict()
  {
    var car = Car.Create("SUV", 2023, 42000m);
    var carRepository = new TestCarRepository(car);
    var existingListing = MarketplaceListing.Create(car.Id, 41000m, null);
    var listingRepository = new TestMarketplaceListingRepository(existingListing);
    var service = new MarketplaceService(listingRepository, carRepository, new TrackingUnitOfWork());

    await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(new CreateMarketplaceListingRequest(car.Id, 40000m, null)));
  }

  [Fact]
  public async Task MarkAsSoldAsync_WithActiveListing_SetsStatusToSold()
  {
    var car = Car.Create("Coupe", 2022, 38000m);
    var carRepository = new TestCarRepository(car);
    var listing = MarketplaceListing.Create(car.Id, 36000m, null);
    var listingRepository = new TestMarketplaceListingRepository(listing);
    var service = new MarketplaceService(listingRepository, carRepository, new TrackingUnitOfWork());

    var result = await service.MarkAsSoldAsync(listing.Id);

    Assert.Equal(MarketplaceListingStatus.Sold, result.Status);
    Assert.Equal(MarketplaceListingStatus.Sold, listingRepository.Listings.Single().Status);
  }

  [Fact]
  public async Task UpdateAsync_WithValidData_UpdatesListing()
  {
    var car = Car.Create("Hatchback", 2021, 18000m);
    var carRepository = new TestCarRepository(car);
    var listing = MarketplaceListing.Create(car.Id, 17500m, "Great city car");
    var listingRepository = new TestMarketplaceListingRepository(listing);
    var service = new MarketplaceService(listingRepository, carRepository, new TrackingUnitOfWork());

    var result = await service.UpdateAsync(listing.Id, new UpdateMarketplaceListingRequest(17000m, "Price reduced"));

    Assert.Equal(17000m, result.AskingPrice);
    Assert.Equal("Price reduced", result.Description);
    Assert.Equal(17000m, listingRepository.Listings.Single().AskingPrice);
  }

  [Fact]
  public async Task ArchiveAsync_SetsStatusToArchived()
  {
    var car = Car.Create("Sedan", 2020, 25000m);
    var carRepository = new TestCarRepository(car);
    var listing = MarketplaceListing.Create(car.Id, 24500m, null);
    var listingRepository = new TestMarketplaceListingRepository(listing);
    var service = new MarketplaceService(listingRepository, carRepository, new TrackingUnitOfWork());

    var result = await service.ArchiveAsync(listing.Id);

    Assert.Equal(MarketplaceListingStatus.Archived, result.Status);
    Assert.Equal(MarketplaceListingStatus.Archived, listingRepository.Listings.Single().Status);
  }

  private sealed class TestCarRepository : ICarRepository
  {
    private readonly List<Car> _cars;

    public TestCarRepository(params Car[] cars)
    {
      _cars = cars.ToList();
    }

    public Task AddAsync(Car car, CancellationToken cancellationToken = default)
    {
      _cars.Add(car);
      return Task.CompletedTask;
    }

    public Task DeleteAsync(Car car, CancellationToken cancellationToken = default)
    {
      _cars.RemoveAll(existing => existing.Id == car.Id);
      return Task.CompletedTask;
    }

    public Task<Car?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      var car = _cars.FirstOrDefault(existing => existing.Id == id);
      return Task.FromResult(car);
    }

    public Task<IReadOnlyList<Car>> ListAsync(CancellationToken cancellationToken = default)
    {
      IReadOnlyList<Car> snapshot = _cars.ToList();
      return Task.FromResult(snapshot);
    }

    public Task UpdateAsync(Car car, CancellationToken cancellationToken = default)
    {
      var index = _cars.FindIndex(existing => existing.Id == car.Id);
      if (index >= 0)
      {
        _cars[index] = car;
      }
      return Task.CompletedTask;
    }
  }

  private sealed class TestMarketplaceListingRepository : IMarketplaceListingRepository
  {
    private readonly List<MarketplaceListing> _listings;

    public TestMarketplaceListingRepository(params MarketplaceListing[] listings)
    {
      _listings = listings.ToList();
    }

    public IReadOnlyCollection<MarketplaceListing> Listings => _listings.AsReadOnly();

    public Task AddAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
    {
      _listings.Add(listing);
      return Task.CompletedTask;
    }

    public Task DeleteAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
    {
      _listings.RemoveAll(existing => existing.Id == listing.Id);
      return Task.CompletedTask;
    }

    public Task<MarketplaceListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      var listing = _listings.FirstOrDefault(existing => existing.Id == id);
      return Task.FromResult(listing);
    }

    public Task<IReadOnlyList<MarketplaceListing>> ListAsync(CancellationToken cancellationToken = default)
    {
      IReadOnlyList<MarketplaceListing> snapshot = _listings.ToList();
      return Task.FromResult(snapshot);
    }

    public Task<IReadOnlyList<MarketplaceListing>> ListByCarIdAsync(Guid carId, CancellationToken cancellationToken = default)
    {
      IReadOnlyList<MarketplaceListing> snapshot = _listings.Where(existing => existing.CarId == carId).ToList();
      return Task.FromResult(snapshot);
    }

    public Task UpdateAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
    {
      var index = _listings.FindIndex(existing => existing.Id == listing.Id);
      if (index >= 0)
      {
        _listings[index] = listing;
      }
      return Task.CompletedTask;
    }
  }

  private sealed class TrackingUnitOfWork : IUnitOfWork
  {
    public bool SaveChangesCalled { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      SaveChangesCalled = true;
      return Task.FromResult(1);
    }
  }
}
