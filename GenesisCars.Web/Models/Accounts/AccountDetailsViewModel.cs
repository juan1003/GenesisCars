using GenesisCars.Application.Accounts;

namespace GenesisCars.Web.Models.Accounts;

public class AccountDetailsViewModel
{
  public AccountDto Account { get; set; } = null!;

  public AccountAmountInputModel Credit { get; set; } = new();

  public AccountAmountInputModel Debit { get; set; } = new();

  public AccountTransferInputModel Transfer { get; set; } = new();

  public IReadOnlyCollection<AccountDto> OtherAccounts { get; set; } = Array.Empty<AccountDto>();
}
