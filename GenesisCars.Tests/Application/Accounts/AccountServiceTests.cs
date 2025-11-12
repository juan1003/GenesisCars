using GenesisCars.Application.Accounts;
using GenesisCars.Application.Exceptions;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Exceptions;
using GenesisCars.Domain.Repositories;

namespace GenesisCars.Tests.Application.Accounts;

public class AccountServiceTests
{
  [Fact]
  public async Task CreateAsync_WithValidRequest_PersistsAccount()
  {
    var repository = new TestAccountRepository();
    var unitOfWork = new TrackingUnitOfWork();
    var service = new AccountService(repository, unitOfWork);

    var result = await service.CreateAsync(new CreateAccountRequest("Alice", 100m));

    Assert.Equal("Alice", result.OwnerName);
    Assert.Equal(100m, result.Balance);
    Assert.Single(repository.Accounts);
    Assert.True(unitOfWork.SaveChangesCalled);
    Assert.NotEmpty(result.Transactions);
    Assert.Contains(result.Transactions, t => t.Type == "Credit" && t.Amount == 100m);
  }

  [Fact]
  public async Task CreditAsync_WithExistingAccount_UpdatesBalance()
  {
    var existing = Account.Create("Bob", 50m);
    var repository = new TestAccountRepository(existing);
    var service = new AccountService(repository, new TrackingUnitOfWork());

    var result = await service.CreditAsync(existing.Id, new CreditAccountRequest(25m));

    Assert.Equal(75m, result.Balance);
    Assert.Equal(75m, repository.Accounts.Single().Balance);
    Assert.NotEmpty(result.Transactions);
    Assert.Equal("Credit", result.Transactions.First().Type);
  }

  [Fact]
  public async Task DebitAsync_WithInsufficientFunds_ThrowsDomainException()
  {
    var existing = Account.Create("Charlie", 30m);
    var repository = new TestAccountRepository(existing);
    var service = new AccountService(repository, new TrackingUnitOfWork());

    await Assert.ThrowsAsync<DomainException>(() => service.DebitAsync(existing.Id, new DebitAccountRequest(60m)));
    var current = await service.GetByIdAsync(existing.Id);
    Assert.NotNull(current);
    Assert.NotEmpty(current!.Transactions);
  }

  [Fact]
  public async Task TransferAsync_WithValidAccounts_TransfersFunds()
  {
    var source = Account.Create("Diane", 200m);
    var recipient = Account.Create("Eve", 50m);
    var repository = new TestAccountRepository(source, recipient);
    var service = new AccountService(repository, new TrackingUnitOfWork());

    var result = await service.TransferAsync(source.Id, new TransferFundsRequest(recipient.Id, 75m));

    Assert.Equal(125m, result.Source.Balance);
    Assert.Equal(125m, repository.Accounts.Single(a => a.Id == source.Id).Balance);
    Assert.Equal(125m, result.Recipient.Balance);
    Assert.Equal(125m, repository.Accounts.Single(a => a.Id == recipient.Id).Balance);
    Assert.Contains(result.Source.Transactions, t => t.Type == "Debit" && t.Amount == 75m);
    Assert.Contains(result.Recipient.Transactions, t => t.Type == "Credit" && t.Amount == 75m);
  }

  [Fact]
  public async Task TransferAsync_WithMissingRecipient_ThrowsNotFound()
  {
    var source = Account.Create("Frank", 40m);
    var repository = new TestAccountRepository(source);
    var service = new AccountService(repository, new TrackingUnitOfWork());

    var missingId = Guid.NewGuid();

    await Assert.ThrowsAsync<NotFoundException>(() => service.TransferAsync(source.Id, new TransferFundsRequest(missingId, 10m)));
  }

  private sealed class TestAccountRepository : IAccountRepository
  {
    private readonly List<Account> _accounts;

    public TestAccountRepository(params Account[] seed)
    {
      _accounts = seed.ToList();
    }

    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

    public Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
      _accounts.Add(account);
      return Task.CompletedTask;
    }

    public Task DeleteAsync(Account account, CancellationToken cancellationToken = default)
    {
      _accounts.RemoveAll(existing => existing.Id == account.Id);
      return Task.CompletedTask;
    }

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      var account = _accounts.FirstOrDefault(existing => existing.Id == id);
      return Task.FromResult(account);
    }

    public Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken = default)
    {
      IReadOnlyList<Account> snapshot = _accounts.ToList();
      return Task.FromResult(snapshot);
    }

    public Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
      var index = _accounts.FindIndex(existing => existing.Id == account.Id);
      if (index >= 0)
      {
        _accounts[index] = account;
      }
      return Task.CompletedTask;
    }
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
