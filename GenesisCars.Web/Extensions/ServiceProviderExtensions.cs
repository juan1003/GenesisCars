using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GenesisCars.Web.Extensions;

public static class ServiceProviderExtensions
{
  private const int RequiredInventoryCount = 40;

  private static readonly (string Model, int Year, decimal Price)[] InventorySeed = new[]
  {
    ("Tesla Model 3 Long Range", 2024, 52990m),
    ("Tesla Model Y Performance", 2024, 58990m),
    ("Ford Mustang Mach-E Premium", 2023, 47995m),
    ("Ford F-150 Lightning Lariat", 2023, 68999m),
    ("Chevrolet Silverado 1500 RST", 2022, 49875m),
    ("Chevrolet Corvette Stingray", 2024, 65995m),
    ("Toyota RAV4 Hybrid XSE", 2023, 37450m),
    ("Toyota Camry XSE", 2024, 33495m),
    ("Toyota Tacoma TRD Pro", 2023, 49655m),
    ("Honda Accord Touring", 2024, 37895m),
    ("Honda CR-V Hybrid Sport Touring", 2023, 38950m),
    ("Honda Civic Type R", 2024, 43995m),
    ("Hyundai Ioniq 5 Limited", 2023, 52495m),
    ("Hyundai Palisade Calligraphy", 2024, 50900m),
    ("Hyundai Kona Electric Limited", 2023, 41800m),
    ("Kia EV6 GT-Line", 2024, 56200m),
    ("Kia Telluride SX Prestige", 2023, 51985m),
    ("Subaru Outback Wilderness", 2024, 39390m),
    ("Subaru Crosstrek Limited", 2024, 32995m),
    ("Mazda CX-90 Turbo S Premium", 2024, 57895m),
    ("Mazda MX-5 Miata Club", 2024, 32415m),
    ("BMW i4 eDrive40", 2024, 61900m),
    ("BMW X5 xDrive50e", 2024, 72900m),
    ("Mercedes-Benz EQE Sedan 350+", 2024, 76800m),
    ("Mercedes-Benz GLE 450", 2023, 67900m),
    ("Audi Q4 e-tron Premium Plus", 2024, 55995m),
    ("Audi A6 Allroad Prestige", 2023, 69700m),
    ("Volkswagen ID.4 Pro S", 2024, 48995m),
    ("Volkswagen Atlas Cross Sport SEL", 2023, 47995m),
    ("Volvo XC60 Recharge Ultimate", 2024, 68950m),
    ("Volvo C40 Recharge Plus", 2024, 57450m),
    ("Lexus RX 500h F SPORT", 2023, 63800m),
    ("Lexus NX 350h Luxury", 2024, 48925m),
    ("Nissan Ariya Platinum+", 2024, 59890m),
    ("Nissan Z Performance", 2024, 51990m),
    ("Porsche Macan T", 2023, 63900m),
    ("Porsche Taycan 4S", 2024, 109000m),
    ("Genesis GV70 Electrified", 2024, 65750m),
    ("Genesis G80 Sport Prestige", 2024, 71825m),
    ("Rivian R1T Adventure", 2024, 78900m),
    ("Rivian R1S Adventure", 2024, 84900m),
    ("Jaguar F-PACE SVR", 2023, 92400m),
    ("Land Rover Defender 110 X-Dynamic", 2024, 78900m),
    ("Mini Cooper SE Iconic", 2024, 34900m),
    ("Cadillac Lyriq Luxury AWD", 2024, 62990m),
    ("Cadillac XT6 Sport", 2023, 60190m),
    ("Jeep Grand Cherokee 4xe Trailhawk", 2024, 67995m),
    ("Jeep Wrangler Rubicon 392", 2024, 89995m),
    ("Lucid Air Touring", 2024, 98400m),
    ("Polestar 2 Long Range Dual Motor", 2024, 56900m)
  };

  public static async Task EnsureInventorySeedAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
  {
    using var scope = services.CreateScope();
    var carRepository = scope.ServiceProvider.GetRequiredService<ICarRepository>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    var existingCars = await carRepository.ListAsync(cancellationToken).ConfigureAwait(false);
    if (existingCars.Count >= RequiredInventoryCount)
    {
      return;
    }

    var carsNeeded = RequiredInventoryCount - existingCars.Count;
    var index = 0;

    while (carsNeeded > 0 && index < InventorySeed.Length)
    {
      var spec = InventorySeed[index];
      var car = Car.Create(spec.Model, spec.Year, spec.Price);
      await carRepository.AddAsync(car, cancellationToken).ConfigureAwait(false);
      carsNeeded--;
      index++;
    }

    while (carsNeeded > 0)
    {
      // Fallback in case the inventory list is exhausted; reuse the last spec with slight adjustments.
      var spec = InventorySeed[^1];
      var adjustedPrice = spec.Price + carsNeeded * 100m;
      var car = Car.Create($"{spec.Model} {carsNeeded}", spec.Year, adjustedPrice);
      await carRepository.AddAsync(car, cancellationToken).ConfigureAwait(false);
      carsNeeded--;
    }

    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
  }
}
