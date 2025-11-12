using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Exceptions;

namespace GenesisCars.Tests.Domain.Entities;

public class AccountTests
{
  [Fact]
  public void Transfer_Succeeds_WhenSufficientFunds()
  {
    // Given Alice has $100 and Bob has $50
    var alice = Account.Create("Alice", 100m);
    var bob = Account.Create("Bob", 50m);

    // When Alice transfers $20 to Bob
    alice.TransferTo(bob, 20m);

    // Then Alice should have $80 and Bob should have $70
    Assert.Equal(80m, alice.Balance);
    Assert.Equal(70m, bob.Balance);
    Assert.Contains(alice.Transactions, t => t.Type == "Debit" && t.Amount == 20m);
    Assert.Contains(bob.Transactions, t => t.Type == "Credit" && t.Amount == 20m);
  }

  [Fact]
  public void Transfer_Throws_WhenInsufficientFunds()
  {
    var alice = Account.Create("Alice", 10m);
    var bob = Account.Create("Bob", 0m);

    var ex = Assert.Throws<DomainException>(() => alice.TransferTo(bob, 20m));
    Assert.Equal("Insufficient funds.", ex.Message);
  }

  [Fact]
  public void CreditAndDebit_WorkAsExpected()
  {
    var acct = Account.Create("Tester", 0m);
    acct.Credit(25.5m);
    Assert.Equal(25.5m, acct.Balance);
    acct.Debit(5.25m);
    Assert.Equal(20.25m, acct.Balance);
    Assert.Equal(3, acct.Transactions.Count);
  }
}
