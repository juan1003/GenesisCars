namespace GenesisCars.Application.Accounts;

public interface IAccountService
{
  Task<IReadOnlyCollection<AccountDto>> GetAllAsync(CancellationToken cancellationToken = default);

  Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);

  Task<AccountDto> CreditAsync(Guid id, CreditAccountRequest request, CancellationToken cancellationToken = default);

  Task<AccountDto> DebitAsync(Guid id, DebitAccountRequest request, CancellationToken cancellationToken = default);

  Task<TransferResultDto> TransferAsync(Guid sourceAccountId, TransferFundsRequest request, CancellationToken cancellationToken = default);

  Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
