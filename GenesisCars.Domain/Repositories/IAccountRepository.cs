using GenesisCars.Domain.Entities;

namespace GenesisCars.Domain.Repositories;

public interface IAccountRepository
{
  Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken = default);

  Task AddAsync(Account account, CancellationToken cancellationToken = default);

  Task UpdateAsync(Account account, CancellationToken cancellationToken = default);

  Task DeleteAsync(Account account, CancellationToken cancellationToken = default);
}
