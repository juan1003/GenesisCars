using GenesisCars.Domain.Repositories;

namespace GenesisCars.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
  private readonly IUserRepository _userRepository;
  private readonly ICarRepository _carRepository;

  public DashboardService(IUserRepository userRepository, ICarRepository carRepository)
  {
    _userRepository = userRepository;
    _carRepository = carRepository;
  }

  public async Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
  {
    var usersTask = _userRepository.ListAsync(cancellationToken);
    var carsTask = _carRepository.ListAsync(cancellationToken);

    await Task.WhenAll(usersTask, carsTask).ConfigureAwait(false);

    var users = await usersTask.ConfigureAwait(false);
    var cars = await carsTask.ConfigureAwait(false);

    var totalUsers = users.Count;
    var totalCars = cars.Count;
    var totalValue = cars.Sum(car => car.Price);
    var averagePrice = totalCars > 0 ? totalValue / totalCars : 0m;

    return new DashboardMetricsDto(
        totalUsers,
        totalCars,
        decimal.Round(averagePrice, 2, MidpointRounding.AwayFromZero),
        decimal.Round(totalValue, 2, MidpointRounding.AwayFromZero),
        DateTime.UtcNow);
  }
}
