using GenesisCars.Application.Accounts;
using GenesisCars.Application.Auth;
using GenesisCars.Application.Dashboard;
using GenesisCars.Application.Inventory;
using GenesisCars.Application.Marketplace;
using GenesisCars.Application.Payments;
using GenesisCars.Application.Recommendations;
using GenesisCars.Application.Users;
using GenesisCars.Domain.Services;
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
    services.AddScoped<IAccountService, AccountService>();
    services.AddScoped<IMarketplaceService, MarketplaceService>();
    services.AddScoped<IPaymentService, PaymentService>();
    services.AddSingleton<ICarRecommendationEngine, CarRecommendationEngine>();
    services.AddScoped<IRecommendationService, RecommendationService>();
    return services;
  }
}
