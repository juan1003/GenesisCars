namespace GenesisCars.Application.Dashboard;

public interface IDashboardService
{
  Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
}
