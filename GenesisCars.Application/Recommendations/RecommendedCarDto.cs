namespace GenesisCars.Application.Recommendations;

public sealed record RecommendedCarDto(
    Guid CarId,
    string Model,
    int Year,
    decimal Price,
    decimal MatchScore,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
