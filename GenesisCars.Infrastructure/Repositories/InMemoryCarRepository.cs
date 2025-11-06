using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Infrastructure.Repositories;

public sealed class InMemoryCarRepository : ICarRepository
{
  private readonly List<Car> _cars = new();
  private readonly SemaphoreSlim _lock = new(1, 1);

  public async Task AddAsync(Car car, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      _cars.Add(car);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task DeleteAsync(Car car, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      _cars.RemoveAll(c => c.Id == car.Id);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<Car?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _cars.FirstOrDefault(c => c.Id == id);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<IReadOnlyList<Car>> ListAsync(CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _cars
          .OrderByDescending(c => c.Year)
          .ThenBy(c => c.Model)
          .ToList();
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task UpdateAsync(Car car, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      var index = _cars.FindIndex(c => c.Id == car.Id);
      if (index >= 0)
      {
        _cars[index] = car;
      }
    }
    finally
    {
      _lock.Release();
    }
  }
}
