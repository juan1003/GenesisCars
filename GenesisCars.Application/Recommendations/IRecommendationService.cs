namespace GenesisCars.Application.Recommendations;

public interface IRecommendationService
{
  Task<IReadOnlyCollection<RecommendedCarDto>> GetRecommendationsAsync(
      RecommendationRequest request,
      CancellationToken cancellationToken = default);
}
