using GenesisCars.Application.Auth;
using GenesisCars.Application.Dashboard;
using GenesisCars.Application.Inventory;
using GenesisCars.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace GenesisCars.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<ICarService, CarService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IDashboardService, DashboardService>();
    services.AddScoped<IUserService, UserService>();
    return services;
  }
}
