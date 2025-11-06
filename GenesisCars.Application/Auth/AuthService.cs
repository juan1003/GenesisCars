using GenesisCars.Application.Exceptions;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Application.Auth;

public sealed class AuthService : IAuthService
{
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;

  public AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork)
  {
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<AuthenticatedUserDto?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
  {
    if (request is null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    Email email;
    try
    {
      email = Email.Create(request.Email);
    }
    catch
    {
      return null;
    }

    var user = await _userRepository.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
    if (user is null)
    {
      return null;
    }

    if (!string.Equals(user.LastName, request.LastName, StringComparison.OrdinalIgnoreCase))
    {
      return null;
    }

    return new AuthenticatedUserDto(user.Id, user.FirstName, user.LastName, user.Email.Value);
  }

  public async Task<AuthenticatedUserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
  {
    if (request is null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    var email = Email.Create(request.Email);
    var existing = await _userRepository.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
    if (existing is not null)
    {
      throw new ConflictException($"A user with email '{email}' already exists.");
    }

    var user = User.Create(request.FirstName, request.LastName, email);

    await _userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return new AuthenticatedUserDto(user.Id, user.FirstName, user.LastName, user.Email.Value);
  }
}
