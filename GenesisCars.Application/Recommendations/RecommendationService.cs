using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;
using GenesisCars.Domain.Services;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Application.Recommendations;

public sealed class RecommendationService : IRecommendationService
{
  private const int MaxLimit = 20;

  private readonly ICarRepository _carRepository;
  private readonly IMarketplaceListingRepository _listingRepository;
  private readonly ICarRecommendationEngine _engine;

  public RecommendationService(
      ICarRepository carRepository,
      IMarketplaceListingRepository listingRepository,
      ICarRecommendationEngine engine)
  {
    _carRepository = carRepository;
    _listingRepository = listingRepository;
    _engine = engine;
  }

  public async Task<IReadOnlyCollection<RecommendedCarDto>> GetRecommendationsAsync(
      RecommendationRequest request,
      CancellationToken cancellationToken = default)
  {
    if (request is null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    var effectiveLimit = Math.Clamp(request.Limit <= 0 ? 5 : request.Limit, 1, MaxLimit);

    var cars = await _carRepository.ListAsync(cancellationToken).ConfigureAwait(false);
    if (cars.Count == 0)
    {
      return Array.Empty<RecommendedCarDto>();
    }

    var listings = await _listingRepository.ListAsync(cancellationToken).ConfigureAwait(false);
    var unavailableCarIds = listings
        .Where(listing => listing.Status == MarketplaceListingStatus.Active)
        .Select(listing => listing.CarId)
        .ToHashSet();

    var availableCars = cars
        .Where(car => !unavailableCarIds.Contains(car.Id))
        .ToArray();

    if (availableCars.Length == 0)
    {
      return Array.Empty<RecommendedCarDto>();
    }

    var criteria = RecommendationCriteria.Create(request.Budget, request.MinYear);
    var recommendations = _engine.Recommend(availableCars, criteria, effectiveLimit);

    return recommendations
        .Select(result => new RecommendedCarDto(
            result.Car.Id,
            result.Car.Model,
            result.Car.Year,
            result.Car.Price,
            result.Score,
            result.Car.CreatedAtUtc,
            result.Car.UpdatedAtUtc))
        .ToArray();
  }
}
