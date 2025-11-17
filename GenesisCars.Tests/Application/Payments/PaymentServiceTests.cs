using System.Linq;
using GenesisCars.Application.Exceptions;
using GenesisCars.Application.Payments;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Tests.Application.Payments;

public class PaymentServiceTests
{
  [Fact]
  public async Task CreateAsync_WithValidListing_CreatesPaymentIntent()
  {
    var listing = MarketplaceListing.Create(Guid.NewGuid(), 50000m, "Performance package");
    var car = Car.Create("Coupe", 2024, 48000m);
    var listingRepository = new TestMarketplaceListingRepository(listing);
    var paymentRepository = new TestPaymentIntentRepository();
    var carRepository = new TestCarRepository(car);
    var gateway = new TestPaymentGateway();
    var service = new PaymentService(paymentRepository, listingRepository, carRepository, gateway, new TrackingUnitOfWork());

    var result = await service.CreateAsync(new CreatePaymentIntentRequest(listing.Id, "usd"));

    Assert.NotNull(result.ProviderIntentId);
    Assert.NotNull(result.ClientSecret);
    Assert.Equal(PaymentStatus.Pending, result.Status);
    Assert.Single(paymentRepository.Items);
  }

  [Fact]
  public async Task CreateAsync_WithInactiveListing_ThrowsConflict()
  {
    var listing = MarketplaceListing.Create(Guid.NewGuid(), 1000m, null);
    listing.MarkAsSold();
    var listingRepository = new TestMarketplaceListingRepository(listing);
    var paymentRepository = new TestPaymentIntentRepository();
    var carRepository = new TestCarRepository();
    var gateway = new TestPaymentGateway();
    var service = new PaymentService(paymentRepository, listingRepository, carRepository, gateway, new TrackingUnitOfWork());

    await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(new CreatePaymentIntentRequest(listing.Id, "usd")));
  }

  [Fact]
  public async Task ConfirmAsync_WithPendingIntent_MarksAsSucceeded()
  {
    var listing = MarketplaceListing.Create(Guid.NewGuid(), 2000m, null);
    var gateway = new TestPaymentGateway();
    var payment = PaymentIntent.Create(listing.Id, listing.AskingPrice, "USD");
    var gatewayResult = await gateway.CreatePaymentIntentAsync(payment.Amount, payment.Currency, "Test payment");
    payment.ApplyProviderDetails(gatewayResult.ProviderIntentId, gatewayResult.ClientSecret);

    var listingRepository = new TestMarketplaceListingRepository(listing);
    var paymentRepository = new TestPaymentIntentRepository(payment);
    var carRepository = new TestCarRepository();
    var service = new PaymentService(paymentRepository, listingRepository, carRepository, gateway, new TrackingUnitOfWork());

    var result = await service.ConfirmAsync(payment.Id);

    Assert.Equal(PaymentStatus.Succeeded, result.Status);
    Assert.Equal(PaymentStatus.Succeeded, paymentRepository.Items.Single().Status);
  }

  [Fact]
  public async Task CancelAsync_WithPendingIntent_MarksAsCanceled()
  {
    var listing = MarketplaceListing.Create(Guid.NewGuid(), 2000m, null);
    var gateway = new TestPaymentGateway();
    var payment = PaymentIntent.Create(listing.Id, listing.AskingPrice, "USD");
    var gatewayResult = await gateway.CreatePaymentIntentAsync(payment.Amount, payment.Currency, "Test payment");
    payment.ApplyProviderDetails(gatewayResult.ProviderIntentId, gatewayResult.ClientSecret);

    var listingRepository = new TestMarketplaceListingRepository(listing);
    var paymentRepository = new TestPaymentIntentRepository(payment);
    var carRepository = new TestCarRepository();
    var service = new PaymentService(paymentRepository, listingRepository, carRepository, gateway, new TrackingUnitOfWork());

    var result = await service.CancelAsync(payment.Id);

    Assert.Equal(PaymentStatus.Canceled, result.Status);
    Assert.Equal(PaymentStatus.Canceled, paymentRepository.Items.Single().Status);
  }

  private sealed class TestMarketplaceListingRepository : IMarketplaceListingRepository
  {
    private readonly MarketplaceListing _listing;

    public TestMarketplaceListingRepository(MarketplaceListing listing)
    {
      _listing = listing;
    }

    public Task AddAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
    {
      throw new NotImplementedException();
    }

    public Task DeleteAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
    {
      throw new NotImplementedException();
    }

    public Task<MarketplaceListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      return Task.FromResult(id == _listing.Id ? _listing : null);
    }

    public Task<IReadOnlyList<MarketplaceListing>> ListAsync(CancellationToken cancellationToken = default)
    {
      return Task.FromResult<IReadOnlyList<MarketplaceListing>>(new[] { _listing });
    }

    public Task<IReadOnlyList<MarketplaceListing>> ListByCarIdAsync(Guid carId, CancellationToken cancellationToken = default)
    {
      return Task.FromResult<IReadOnlyList<MarketplaceListing>>(Array.Empty<MarketplaceListing>());
    }

    public Task UpdateAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
    {
      throw new NotImplementedException();
    }
  }

  private sealed class TestPaymentIntentRepository : IPaymentIntentRepository
  {
    private readonly List<PaymentIntent> _items;

    public TestPaymentIntentRepository(params PaymentIntent[] seed)
    {
      _items = seed.ToList();
    }

    public IReadOnlyCollection<PaymentIntent> Items => _items.AsReadOnly();

    public Task AddAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken = default)
    {
      _items.Add(paymentIntent);
      return Task.CompletedTask;
    }

    public Task UpdateAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken = default)
    {
      var index = _items.FindIndex(existing => existing.Id == paymentIntent.Id);
      if (index >= 0)
      {
        _items[index] = paymentIntent;
      }
      return Task.CompletedTask;
    }

    public Task<PaymentIntent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      var paymentIntent = _items.FirstOrDefault(existing => existing.Id == id);
      return Task.FromResult(paymentIntent);
    }

    public Task<IReadOnlyList<PaymentIntent>> ListByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
      IReadOnlyList<PaymentIntent> snapshot = _items.Where(existing => existing.ListingId == listingId).ToList();
      return Task.FromResult(snapshot);
    }
  }

  private sealed class TestCarRepository : ICarRepository
  {
    private readonly List<Car> _cars;

    public TestCarRepository(params Car[] cars)
    {
      _cars = cars.ToList();
    }

    public Task AddAsync(Car car, CancellationToken cancellationToken = default)
    {
      _cars.Add(car);
      return Task.CompletedTask;
    }

    public Task DeleteAsync(Car car, CancellationToken cancellationToken = default)
    {
      _cars.RemoveAll(existing => existing.Id == car.Id);
      return Task.CompletedTask;
    }

    public Task<Car?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      var car = _cars.FirstOrDefault(existing => existing.Id == id);
      return Task.FromResult(car);
    }

    public Task<IReadOnlyList<Car>> ListAsync(CancellationToken cancellationToken = default)
    {
      IReadOnlyList<Car> snapshot = _cars.ToList();
      return Task.FromResult(snapshot);
    }

    public Task UpdateAsync(Car car, CancellationToken cancellationToken = default)
    {
      var index = _cars.FindIndex(existing => existing.Id == car.Id);
      if (index >= 0)
      {
        _cars[index] = car;
      }
      return Task.CompletedTask;
    }
  }

  private sealed class TestPaymentGateway : IPaymentGateway
  {
    private readonly HashSet<string> _created = new();

    public Task<PaymentGatewayCreateResult> CreatePaymentIntentAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default)
    {
      var providerId = $"pi_{Guid.NewGuid():N}";
      _created.Add(providerId);
      return Task.FromResult(new PaymentGatewayCreateResult(providerId, $"{providerId}_secret"));
    }

    public Task ConfirmPaymentIntentAsync(string providerIntentId, CancellationToken cancellationToken = default)
    {
      if (!_created.Contains(providerIntentId))
      {
        throw new InvalidOperationException("Unknown provider intent id.");
      }

      return Task.CompletedTask;
    }

    public Task CancelPaymentIntentAsync(string providerIntentId, CancellationToken cancellationToken = default)
    {
      if (!_created.Contains(providerIntentId))
      {
        throw new InvalidOperationException("Unknown provider intent id.");
      }

      return Task.CompletedTask;
    }
  }

  private sealed class TrackingUnitOfWork : IUnitOfWork
  {
    public bool SaveChangesCalled { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      SaveChangesCalled = true;
      return Task.FromResult(1);
    }
  }
}
