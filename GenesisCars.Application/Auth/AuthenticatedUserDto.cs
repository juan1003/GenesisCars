namespace GenesisCars.Application.Auth;

public sealed record AuthenticatedUserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
);
