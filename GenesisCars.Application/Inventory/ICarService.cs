namespace GenesisCars.Application.Inventory;

public interface ICarService
{
  Task<IReadOnlyCollection<CarDto>> GetAllAsync(CancellationToken cancellationToken = default);

  Task<CarDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  Task<CarDto> CreateAsync(CreateCarRequest request, CancellationToken cancellationToken = default);

  Task<CarDto> UpdateAsync(Guid id, UpdateCarRequest request, CancellationToken cancellationToken = default);

  Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
