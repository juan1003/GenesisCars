namespace GenesisCars.Application.Inventory;

public sealed record CarDto(
    Guid Id,
    string Model,
    int Year,
    decimal Price,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);
