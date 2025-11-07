using System.Linq;
using GenesisCars.Application.Dashboard;
using GenesisCars.Web.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenesisCars.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
  private readonly IDashboardService _dashboardService;

  public DashboardController(IDashboardService dashboardService)
  {
    _dashboardService = dashboardService;
  }

  public async Task<IActionResult> Index(CancellationToken cancellationToken)
  {
    var metrics = await _dashboardService.GetMetricsAsync(cancellationToken);

    var viewModel = new DashboardViewModel
    {
      TotalUsers = metrics.TotalUsers,
      TotalCars = metrics.TotalCars,
      AverageCarPrice = metrics.AverageCarPrice,
      TotalInventoryValue = metrics.TotalInventoryValue,
      GeneratedAtUtc = metrics.GeneratedAtUtc,
      CarPriceBreakdown = metrics.CarPriceBreakdown
        .Select(slice => new CarPriceSliceViewModel
        {
          Label = slice.Label,
          Price = slice.Price
        })
        .ToList()
    };

    return View(viewModel);
  }
}
