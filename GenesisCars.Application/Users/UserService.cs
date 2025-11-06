using GenesisCars.Application.Exceptions;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Application.Users;

public sealed class UserService : IUserService
{
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;

  public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
  {
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    var users = await _userRepository.ListAsync(cancellationToken);
    return users.Select(MapToDto).ToArray();
  }

  public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var user = await _userRepository.GetByIdAsync(id, cancellationToken);
    return user is null ? null : MapToDto(user);
  }

  public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
  {
    var email = Email.Create(request.Email);
    var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
    if (existingUser is not null)
    {
      throw new ConflictException($"A user with email '{email}' already exists.");
    }

    var user = User.Create(request.FirstName, request.LastName, email);

    await _userRepository.AddAsync(user, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return MapToDto(user);
  }

  public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
  {
    var user = await _userRepository.GetByIdAsync(id, cancellationToken);
    if (user is null)
    {
      throw new NotFoundException($"User '{id}' was not found.");
    }

    var newEmail = Email.Create(request.Email);
    if (!string.Equals(user.Email.Value, newEmail.Value, StringComparison.OrdinalIgnoreCase))
    {
      var emailOwner = await _userRepository.GetByEmailAsync(newEmail, cancellationToken);
      if (emailOwner is not null && emailOwner.Id != user.Id)
      {
        throw new ConflictException($"A user with email '{newEmail}' already exists.");
      }
    }

    user.Update(request.FirstName, request.LastName, newEmail);
    await _userRepository.UpdateAsync(user, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return MapToDto(user);
  }

  public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var user = await _userRepository.GetByIdAsync(id, cancellationToken);
    if (user is null)
    {
      throw new NotFoundException($"User '{id}' was not found.");
    }

    await _userRepository.DeleteAsync(user, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
  }

  private static UserDto MapToDto(User user)
  {
    return new UserDto(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email.Value,
        user.CreatedAtUtc,
        user.UpdatedAtUtc);
  }
}
