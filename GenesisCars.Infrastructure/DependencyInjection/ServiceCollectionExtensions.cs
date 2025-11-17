using GenesisCars.Application.Payments;
using GenesisCars.Domain.Repositories;
using GenesisCars.Infrastructure.Payments;
using GenesisCars.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GenesisCars.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services)
  {
    services.AddSingleton<ICarRepository, InMemoryCarRepository>();
    services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
    services.AddSingleton<IMarketplaceListingRepository, InMemoryMarketplaceListingRepository>();
    services.AddSingleton<IPaymentIntentRepository, InMemoryPaymentIntentRepository>();
    services.AddSingleton<IPaymentGateway, InMemoryStripePaymentGateway>();
    services.AddSingleton<IUnitOfWork, NoOpUnitOfWork>();
    return services;
  }
}
