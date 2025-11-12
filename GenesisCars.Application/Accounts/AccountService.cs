using GenesisCars.Application.Exceptions;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Application.Accounts;

public sealed class AccountService : IAccountService
{
  private readonly IAccountRepository _accountRepository;
  private readonly IUnitOfWork _unitOfWork;

  public AccountService(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
  {
    _accountRepository = accountRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IReadOnlyCollection<AccountDto>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    var accounts = await _accountRepository.ListAsync(cancellationToken).ConfigureAwait(false);
    return accounts.Select(MapToDto).ToArray();
  }

  public async Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var account = await _accountRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    return account is null ? null : MapToDto(account);
  }

  public async Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
  {
    if (request is null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    var account = Account.Create(request.OwnerName, request.InitialBalance);

    await _accountRepository.AddAsync(account, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return MapToDto(account);
  }

  public async Task<AccountDto> CreditAsync(Guid id, CreditAccountRequest request, CancellationToken cancellationToken = default)
  {
    if (request is null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    var account = await _accountRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (account is null)
    {
      throw new NotFoundException($"Account '{id}' was not found.");
    }

    account.Credit(request.Amount);

    await _accountRepository.UpdateAsync(account, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return MapToDto(account);
  }

  public async Task<AccountDto> DebitAsync(Guid id, DebitAccountRequest request, CancellationToken cancellationToken = default)
  {
    if (request is null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    var account = await _accountRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (account is null)
    {
      throw new NotFoundException($"Account '{id}' was not found.");
    }

    account.Debit(request.Amount);

    await _accountRepository.UpdateAsync(account, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return MapToDto(account);
  }

  public async Task<TransferResultDto> TransferAsync(Guid sourceAccountId, TransferFundsRequest request, CancellationToken cancellationToken = default)
  {
    if (request is null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    if (sourceAccountId == request.RecipientAccountId)
    {
      throw new ConflictException("Source and recipient accounts must be different.");
    }

    var source = await _accountRepository.GetByIdAsync(sourceAccountId, cancellationToken).ConfigureAwait(false);
    if (source is null)
    {
      throw new NotFoundException($"Account '{sourceAccountId}' was not found.");
    }

    var recipient = await _accountRepository.GetByIdAsync(request.RecipientAccountId, cancellationToken).ConfigureAwait(false);
    if (recipient is null)
    {
      throw new NotFoundException($"Account '{request.RecipientAccountId}' was not found.");
    }

    source.TransferTo(recipient, request.Amount);

    await _accountRepository.UpdateAsync(source, cancellationToken).ConfigureAwait(false);
    await _accountRepository.UpdateAsync(recipient, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return new TransferResultDto(MapToDto(source), MapToDto(recipient));
  }

  public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var account = await _accountRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (account is null)
    {
      throw new NotFoundException($"Account '{id}' was not found.");
    }

    await _accountRepository.DeleteAsync(account, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
  }

  private static AccountDto MapToDto(Account account)
  {
    return new AccountDto(
        account.Id,
        account.OwnerName,
        account.Balance,
        account.CreatedAtUtc,
        account.UpdatedAtUtc,
        account.Transactions
            .OrderByDescending(t => t.TimestampUtc)
            .Select(MapToDto)
            .ToArray());
  }

  private static AccountTransactionDto MapToDto(AccountTransaction transaction)
  {
    return new AccountTransactionDto(
        transaction.Id,
        transaction.TimestampUtc,
        transaction.Type,
        transaction.Amount,
        transaction.BalanceAfter,
        transaction.Description);
  }
}
