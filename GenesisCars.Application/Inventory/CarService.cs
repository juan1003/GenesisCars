using GenesisCars.Application.Exceptions;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Application.Inventory;

public sealed class CarService : ICarService
{
  private readonly ICarRepository _carRepository;
  private readonly IUnitOfWork _unitOfWork;

  public CarService(ICarRepository carRepository, IUnitOfWork unitOfWork)
  {
    _carRepository = carRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IReadOnlyCollection<CarDto>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    var cars = await _carRepository.ListAsync(cancellationToken);
    return cars.Select(MapToDto).ToArray();
  }

  public async Task<CarDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var car = await _carRepository.GetByIdAsync(id, cancellationToken);
    return car is null ? null : MapToDto(car);
  }

  public async Task<CarDto> CreateAsync(CreateCarRequest request, CancellationToken cancellationToken = default)
  {
    var car = Car.Create(request.Model, request.Year, request.Price);

    await _carRepository.AddAsync(car, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return MapToDto(car);
  }

  public async Task<CarDto> UpdateAsync(Guid id, UpdateCarRequest request, CancellationToken cancellationToken = default)
  {
    var car = await _carRepository.GetByIdAsync(id, cancellationToken);
    if (car is null)
    {
      throw new NotFoundException($"Car '{id}' was not found.");
    }

    car.Update(request.Model, request.Year, request.Price);

    await _carRepository.UpdateAsync(car, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return MapToDto(car);
  }

  public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var car = await _carRepository.GetByIdAsync(id, cancellationToken);
    if (car is null)
    {
      throw new NotFoundException($"Car '{id}' was not found.");
    }

    await _carRepository.DeleteAsync(car, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
  }

  private static CarDto MapToDto(Car car)
  {
    return new CarDto(
        car.Id,
        car.Model,
        car.Year,
        car.Price,
        car.CreatedAtUtc,
        car.UpdatedAtUtc);
  }
}
