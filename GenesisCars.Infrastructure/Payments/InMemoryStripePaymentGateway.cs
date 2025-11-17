using System.Collections.Concurrent;
using GenesisCars.Application.Payments;

namespace GenesisCars.Infrastructure.Payments;

public sealed class InMemoryStripePaymentGateway : IPaymentGateway
{
  private readonly ConcurrentDictionary<string, PaymentRecord> _store = new();

  public Task<PaymentGatewayCreateResult> CreatePaymentIntentAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var providerId = $"pi_{Guid.NewGuid():N}";
    var clientSecret = $"{providerId}_secret";

    var record = new PaymentRecord(amount, currency, description, GatewayStatus.Pending);
    _store[providerId] = record;

    return Task.FromResult(new PaymentGatewayCreateResult(providerId, clientSecret));
  }

  public Task ConfirmPaymentIntentAsync(string providerIntentId, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    if (!_store.TryGetValue(providerIntentId, out var record))
    {
      throw new InvalidOperationException($"Payment intent '{providerIntentId}' was not found in gateway.");
    }

    if (record.Status == GatewayStatus.Canceled)
    {
      throw new InvalidOperationException("Canceled gateway payment intents cannot be confirmed.");
    }

    _store[providerIntentId] = record with { Status = GatewayStatus.Succeeded };
    return Task.CompletedTask;
  }

  public Task CancelPaymentIntentAsync(string providerIntentId, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    if (!_store.TryGetValue(providerIntentId, out var record))
    {
      throw new InvalidOperationException($"Payment intent '{providerIntentId}' was not found in gateway.");
    }

    if (record.Status == GatewayStatus.Succeeded)
    {
      throw new InvalidOperationException("Succeeded gateway payment intents cannot be canceled.");
    }

    _store[providerIntentId] = record with { Status = GatewayStatus.Canceled };
    return Task.CompletedTask;
  }

  private sealed record PaymentRecord(decimal Amount, string Currency, string Description, GatewayStatus Status);

  private enum GatewayStatus
  {
    Pending,
    Succeeded,
    Canceled
  }
}
