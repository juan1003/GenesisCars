using GenesisCars.Domain.Repositories;

namespace GenesisCars.Infrastructure.Repositories;

public sealed class NoOpUnitOfWork : IUnitOfWork
{
  public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    return Task.FromResult(0);
  }
}
