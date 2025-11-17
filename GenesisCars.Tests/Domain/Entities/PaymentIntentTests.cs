using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Exceptions;

namespace GenesisCars.Tests.Domain.Entities;

public class PaymentIntentTests
{
  [Fact]
  public void Create_WithValidValues_SetsInitialState()
  {
    var listingId = Guid.NewGuid();
    var payment = PaymentIntent.Create(listingId, 25000m, "usd");

    Assert.Equal(listingId, payment.ListingId);
    Assert.Equal(25000m, payment.Amount);
    Assert.Equal("USD", payment.Currency);
    Assert.Equal(PaymentStatus.Pending, payment.Status);
    Assert.Null(payment.ProviderIntentId);
    Assert.Null(payment.ClientSecret);
  }

  [Fact]
  public void Create_WithInvalidAmount_Throws()
  {
    Assert.Throws<DomainException>(() => PaymentIntent.Create(Guid.NewGuid(), 0, "USD"));
  }

  [Fact]
  public void ApplyProviderDetails_WithEmptyValues_Throws()
  {
    var payment = PaymentIntent.Create(Guid.NewGuid(), 100m, "USD");

    Assert.Throws<DomainException>(() => payment.ApplyProviderDetails(string.Empty, "secret"));
    Assert.Throws<DomainException>(() => payment.ApplyProviderDetails("pi_123", ""));
  }

  [Fact]
  public void MarkAsSucceeded_FromPending_SetsStatus()
  {
    var payment = PaymentIntent.Create(Guid.NewGuid(), 100m, "USD");

    payment.MarkAsSucceeded();

    Assert.Equal(PaymentStatus.Succeeded, payment.Status);
  }

  [Fact]
  public void Cancel_FromPending_SetsStatus()
  {
    var payment = PaymentIntent.Create(Guid.NewGuid(), 100m, "USD");

    payment.Cancel();

    Assert.Equal(PaymentStatus.Canceled, payment.Status);
  }

  [Fact]
  public void Cancel_AfterSuccess_Throws()
  {
    var payment = PaymentIntent.Create(Guid.NewGuid(), 100m, "USD");
    payment.MarkAsSucceeded();

    Assert.Throws<DomainException>(() => payment.Cancel());
  }
}
