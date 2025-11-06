namespace GenesisCars.Application.Users;

public sealed record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);
