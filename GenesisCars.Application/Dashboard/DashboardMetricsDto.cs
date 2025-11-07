using System.Collections.Generic;

namespace GenesisCars.Application.Dashboard;

public sealed record DashboardMetricsDto(
    int TotalUsers,
    int TotalCars,
    decimal AverageCarPrice,
    decimal TotalInventoryValue,
    DateTime GeneratedAtUtc,
    IReadOnlyList<CarPriceSliceDto> CarPriceBreakdown
);
