namespace GenesisCars.Web.Models.Dashboard;

public class DashboardViewModel
{
  public int TotalUsers { get; set; }

  public int TotalCars { get; set; }

  public decimal AverageCarPrice { get; set; }

  public decimal TotalInventoryValue { get; set; }

  public DateTime GeneratedAtUtc { get; set; }

  public IReadOnlyList<CarPriceSliceViewModel> CarPriceBreakdown { get; set; } = Array.Empty<CarPriceSliceViewModel>();
}
