namespace GenesisCars.Application.Payments;

public interface IPaymentGateway
{
  Task<PaymentGatewayCreateResult> CreatePaymentIntentAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default);

  Task ConfirmPaymentIntentAsync(string providerIntentId, CancellationToken cancellationToken = default);

  Task CancelPaymentIntentAsync(string providerIntentId, CancellationToken cancellationToken = default);
}
