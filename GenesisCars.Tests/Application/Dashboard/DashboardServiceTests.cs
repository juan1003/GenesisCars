using GenesisCars.Application.Dashboard;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Tests.Application.Dashboard;

public class DashboardServiceTests
{
  [Fact]
  public async Task GetMetricsAsync_ReturnsAggregatedMetrics()
  {
    var userRepository = new InMemoryUserRepositoryStub(3);
    var carRepository = new InMemoryCarRepositoryStub(
        Car.Create("Model S", 2024, 90000m),
        Car.Create("Model 3", 2023, 45000m));

    var service = new DashboardService(userRepository, carRepository);

    var metrics = await service.GetMetricsAsync();

    Assert.Equal(3, metrics.TotalUsers);
    Assert.Equal(2, metrics.TotalCars);
    Assert.Equal(67500m, metrics.AverageCarPrice);
    Assert.Equal(135000m, metrics.TotalInventoryValue);
    Assert.Collection(metrics.CarPriceBreakdown,
      first =>
      {
        Assert.Contains("Model S", first.Label);
        Assert.Equal(90000m, first.Price);
      },
      second =>
      {
        Assert.Contains("Model 3", second.Label);
        Assert.Equal(45000m, second.Price);
      });
  }

  private sealed class InMemoryUserRepositoryStub : IUserRepository
  {
    private readonly IReadOnlyList<GenesisCars.Domain.Entities.User> _users;

    public InMemoryUserRepositoryStub(int count)
    {
      _users = Enumerable.Range(0, count)
          .Select(i => GenesisCars.Domain.Entities.User.Create(
              $"First{i}",
              $"Last{i}",
              GenesisCars.Domain.ValueObjects.Email.Create($"user{i}@example.com")))
          .ToList();
    }

    public Task AddAsync(GenesisCars.Domain.Entities.User user, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(GenesisCars.Domain.Entities.User user, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<GenesisCars.Domain.Entities.User?> GetByEmailAsync(GenesisCars.Domain.ValueObjects.Email email, CancellationToken cancellationToken = default) => Task.FromResult<GenesisCars.Domain.Entities.User?>(null);

    public Task<GenesisCars.Domain.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<GenesisCars.Domain.Entities.User?>(null);

    public Task<IReadOnlyList<GenesisCars.Domain.Entities.User>> ListAsync(CancellationToken cancellationToken = default)
    {
      return Task.FromResult(_users);
    }

    public Task UpdateAsync(GenesisCars.Domain.Entities.User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
  }

  private sealed class InMemoryCarRepositoryStub : ICarRepository
  {
    private readonly List<Car> _cars;

    public InMemoryCarRepositoryStub(params Car[] cars)
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
      _cars.RemoveAll(c => c.Id == car.Id);
      return Task.CompletedTask;
    }

    public Task<Car?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      return Task.FromResult(_cars.FirstOrDefault(c => c.Id == id));
    }

    public Task<IReadOnlyList<Car>> ListAsync(CancellationToken cancellationToken = default)
    {
      return Task.FromResult<IReadOnlyList<Car>>(_cars);
    }

    public Task UpdateAsync(Car car, CancellationToken cancellationToken = default)
    {
      return Task.CompletedTask;
    }
  }
}
