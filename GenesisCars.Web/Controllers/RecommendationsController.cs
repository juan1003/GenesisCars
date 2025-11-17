using GenesisCars.Application.Recommendations;
using GenesisCars.Domain.Exceptions;
using GenesisCars.Web.Models.Recommendations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenesisCars.Web.Controllers;

[Authorize]
public class RecommendationsController : Controller
{
  private readonly IRecommendationService _recommendationService;

  public RecommendationsController(IRecommendationService recommendationService)
  {
    _recommendationService = recommendationService;
  }

  [HttpGet]
  public IActionResult Index()
  {
    ViewBag.Recommendations = Array.Empty<RecommendedCarDto>();
    return View(new RecommendationInputModel());
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Index(RecommendationInputModel input, CancellationToken cancellationToken)
  {
    IReadOnlyCollection<RecommendedCarDto> recommendations = Array.Empty<RecommendedCarDto>();

    if (ModelState.IsValid)
    {
      try
      {
        var request = new RecommendationRequest(input.Budget, input.MinYear, input.Limit);
        recommendations = await _recommendationService.GetRecommendationsAsync(request, cancellationToken).ConfigureAwait(false);
      }
      catch (DomainException ex)
      {
        ModelState.AddModelError(string.Empty, ex.Message);
      }
    }

    ViewBag.Recommendations = recommendations;
    return View(input);
  }

  [HttpGet("api/recommendations")]
  [Produces("application/json")]
  public async Task<IActionResult> Suggestions(decimal? budget, int? minYear, int limit = 5, CancellationToken cancellationToken = default)
  {
    try
    {
      var request = new RecommendationRequest(budget, minYear, limit);
      var recommendations = await _recommendationService.GetRecommendationsAsync(request, cancellationToken).ConfigureAwait(false);
      return Ok(recommendations);
    }
    catch (DomainException ex)
    {
      return BadRequest(new { error = ex.Message });
    }
  }
}
