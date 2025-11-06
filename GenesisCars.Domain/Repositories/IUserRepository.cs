using GenesisCars.Domain.Entities;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Domain.Repositories;

public interface IUserRepository
{
  Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

  Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default);

  Task AddAsync(User user, CancellationToken cancellationToken = default);

  Task UpdateAsync(User user, CancellationToken cancellationToken = default);

  Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}
