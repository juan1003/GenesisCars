using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Infrastructure.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
  private readonly List<User> _users = new();
  private readonly SemaphoreSlim _lock = new(1, 1);

  public async Task AddAsync(User user, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      _users.Add(user);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      _users.RemoveAll(u => u.Id == user.Id);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _users.FirstOrDefault(u => string.Equals(u.Email.Value, email.Value, StringComparison.OrdinalIgnoreCase));
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _users.FirstOrDefault(u => u.Id == id);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return _users
          .OrderBy(u => u.FirstName)
          .ThenBy(u => u.LastName)
          .ToList();
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      var index = _users.FindIndex(u => u.Id == user.Id);
      if (index >= 0)
      {
        _users[index] = user;
      }
    }
    finally
    {
      _lock.Release();
    }
  }
}
