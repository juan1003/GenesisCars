using GenesisCars.Domain.Entities;

namespace GenesisCars.Domain.Repositories;

public interface ICarRepository
{
  Task<Car?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  Task<IReadOnlyList<Car>> ListAsync(CancellationToken cancellationToken = default);

  Task AddAsync(Car car, CancellationToken cancellationToken = default);

  Task UpdateAsync(Car car, CancellationToken cancellationToken = default);

  Task DeleteAsync(Car car, CancellationToken cancellationToken = default);
}
