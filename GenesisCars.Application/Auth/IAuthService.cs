namespace GenesisCars.Application.Auth;

public interface IAuthService
{
  Task<AuthenticatedUserDto?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);

  Task<AuthenticatedUserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
