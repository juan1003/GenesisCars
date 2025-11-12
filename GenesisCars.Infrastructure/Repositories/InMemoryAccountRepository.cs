using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Infrastructure.Repositories;

public sealed class InMemoryAccountRepository : IAccountRepository
{
  private readonly List<Account> _accounts = new();
  private readonly SemaphoreSlim _lock = new(1, 1);

  public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _accounts.FirstOrDefault(account => account.Id == id);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _accounts
          .OrderBy(account => account.OwnerName, StringComparer.OrdinalIgnoreCase)
          .ToList();
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      _accounts.Add(account);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      var index = _accounts.FindIndex(existing => existing.Id == account.Id);
      if (index >= 0)
      {
        _accounts[index] = account;
      }
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task DeleteAsync(Account account, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      _accounts.RemoveAll(existing => existing.Id == account.Id);
    }
    finally
    {
      _lock.Release();
    }
  }
}
