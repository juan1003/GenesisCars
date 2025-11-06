using GenesisCars.Application.Auth;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Repositories;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Tests.Application.Auth;

public class AuthServiceTests
{
  [Fact]
  public async Task AuthenticateAsync_WithValidCredentials_ReturnsUser()
  {
    var user = User.Create("Jane", "Doe", Email.Create("jane@example.com"));
    var repository = new SingleUserRepository(user);
    var service = new AuthService(repository, new NoOpUnitOfWork());

    var result = await service.AuthenticateAsync(new LoginRequest("jane@example.com", "Doe"));

    Assert.NotNull(result);
    Assert.Equal(user.Id, result!.Id);
  }

  [Fact]
  public async Task AuthenticateAsync_WithInvalidLastName_ReturnsNull()
  {
    var user = User.Create("Jane", "Doe", Email.Create("jane@example.com"));
    var repository = new SingleUserRepository(user);
    var service = new AuthService(repository, new NoOpUnitOfWork());

    var result = await service.AuthenticateAsync(new LoginRequest("jane@example.com", "Wrong"));

    Assert.Null(result);
  }

  [Fact]
  public async Task RegisterAsync_WithUniqueEmail_CreatesUser()
  {
    var repository = new SingleUserRepository(null);
    var unitOfWork = new TrackingUnitOfWork();
    var service = new AuthService(repository, unitOfWork);

    var result = await service.RegisterAsync(new RegisterRequest("John", "Smith", "john@example.com"));

    Assert.NotNull(result);
    Assert.Equal("John", result.FirstName);
    Assert.True(unitOfWork.SaveChangesCalled);
    Assert.Single(repository.Users);
  }

  [Fact]
  public async Task RegisterAsync_WithExistingEmail_ThrowsConflict()
  {
    var existing = User.Create("Jane", "Doe", Email.Create("jane@example.com"));
    var repository = new SingleUserRepository(existing);
    var service = new AuthService(repository, new NoOpUnitOfWork());

    await Assert.ThrowsAsync<GenesisCars.Application.Exceptions.ConflictException>(
        () => service.RegisterAsync(new RegisterRequest("John", "Smith", "jane@example.com")));
  }

  private sealed class SingleUserRepository : IUserRepository
  {
    private readonly List<User> _users;

    public SingleUserRepository(User? user)
    {
      _users = new List<User>();
      if (user is not null)
      {
        _users.Add(user);
      }
    }

    public IReadOnlyCollection<User> Users => _users;

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
      _users.Add(user);
      return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
      var match = _users.FirstOrDefault(u => string.Equals(u.Email.Value, email.Value, StringComparison.OrdinalIgnoreCase));
      return Task.FromResult(match);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default)
    {
      return Task.FromResult<IReadOnlyList<User>>(_users);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
  }

  private sealed class NoOpUnitOfWork : IUnitOfWork
  {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
  }

  private sealed class TrackingUnitOfWork : IUnitOfWork
  {
    public bool SaveChangesCalled { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      SaveChangesCalled = true;
      return Task.FromResult(1);
    }
  }
}
