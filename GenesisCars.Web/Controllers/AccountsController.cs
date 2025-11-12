using GenesisCars.Application.Accounts;
using GenesisCars.Application.Exceptions;
using GenesisCars.Domain.Exceptions;
using GenesisCars.Web.Models.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GenesisCars.Web.Controllers;

[Authorize]
public class AccountsController : Controller
{
  private readonly IAccountService _accountService;

  public AccountsController(IAccountService accountService)
  {
    _accountService = accountService;
  }

  public async Task<IActionResult> Index(CancellationToken cancellationToken)
  {
    var accounts = await _accountService.GetAllAsync(cancellationToken).ConfigureAwait(false);
    return View(accounts);
  }

  public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
  {
    var viewModel = await BuildDetailsViewModelAsync(id, cancellationToken).ConfigureAwait(false);
    if (viewModel is null)
    {
      return NotFound();
    }

    return View(viewModel);
  }

  public IActionResult Create()
  {
    return View(new AccountInputModel());
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(AccountInputModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return View(model);
    }

    try
    {
      var created = await _accountService.CreateAsync(
          new CreateAccountRequest(model.OwnerName, model.InitialBalance),
          cancellationToken).ConfigureAwait(false);

      TempData["StatusMessage"] = "Account created successfully.";
      return RedirectToAction(nameof(Details), new { id = created.Id });
    }
    catch (DomainException ex)
    {
      ModelState.AddModelError(string.Empty, ex.Message);
      return View(model);
    }
  }

  public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
  {
    var account = await _accountService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (account is null)
    {
      return NotFound();
    }

    return View(account);
  }

  [HttpPost, ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
  {
    try
    {
      await _accountService.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
      TempData["StatusMessage"] = "Account deleted.";
      return RedirectToAction(nameof(Index));
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Credit(Guid id, [Bind(Prefix = "Credit")] AccountAmountInputModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return await RedisplayDetailsAsync(id, cancellationToken, credit: model).ConfigureAwait(false);
    }

    try
    {
      await _accountService.CreditAsync(id, new CreditAccountRequest(model.Amount), cancellationToken).ConfigureAwait(false);
      TempData["StatusMessage"] = "Account credited successfully.";
      return RedirectToAction(nameof(Details), new { id });
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
    catch (DomainException ex)
    {
      ModelState.AddModelError("Credit.Amount", ex.Message);
      return await RedisplayDetailsAsync(id, cancellationToken, credit: model).ConfigureAwait(false);
    }
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Debit(Guid id, [Bind(Prefix = "Debit")] AccountAmountInputModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return await RedisplayDetailsAsync(id, cancellationToken, debit: model).ConfigureAwait(false);
    }

    try
    {
      await _accountService.DebitAsync(id, new DebitAccountRequest(model.Amount), cancellationToken).ConfigureAwait(false);
      TempData["StatusMessage"] = "Account debited successfully.";
      return RedirectToAction(nameof(Details), new { id });
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
    catch (DomainException ex)
    {
      ModelState.AddModelError("Debit.Amount", ex.Message);
      return await RedisplayDetailsAsync(id, cancellationToken, debit: model).ConfigureAwait(false);
    }
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Transfer(Guid id, [Bind(Prefix = "Transfer")] AccountTransferInputModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return await RedisplayDetailsAsync(id, cancellationToken, transfer: model).ConfigureAwait(false);
    }

    try
    {
      await _accountService.TransferAsync(id, new TransferFundsRequest(model.RecipientAccountId, model.Amount), cancellationToken).ConfigureAwait(false);
      TempData["StatusMessage"] = "Transfer completed successfully.";
      return RedirectToAction(nameof(Details), new { id });
    }
    catch (NotFoundException ex)
    {
      ModelState.AddModelError(string.Empty, ex.Message);
      return await RedisplayDetailsAsync(id, cancellationToken, transfer: model).ConfigureAwait(false);
    }
    catch (ConflictException ex)
    {
      ModelState.AddModelError("Transfer.RecipientAccountId", ex.Message);
      return await RedisplayDetailsAsync(id, cancellationToken, transfer: model).ConfigureAwait(false);
    }
    catch (DomainException ex)
    {
      ModelState.AddModelError("Transfer.Amount", ex.Message);
      return await RedisplayDetailsAsync(id, cancellationToken, transfer: model).ConfigureAwait(false);
    }
  }

  private async Task<AccountDetailsViewModel?> BuildDetailsViewModelAsync(Guid id, CancellationToken cancellationToken)
  {
    var account = await _accountService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (account is null)
    {
      return null;
    }

    var allAccounts = await _accountService.GetAllAsync(cancellationToken).ConfigureAwait(false);
    var others = allAccounts
        .Where(a => a.Id != account.Id)
        .OrderBy(a => a.OwnerName, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    return new AccountDetailsViewModel
    {
      Account = account,
      OtherAccounts = others
    };
  }

  private async Task<IActionResult> RedisplayDetailsAsync(
      Guid id,
      CancellationToken cancellationToken,
      AccountAmountInputModel? credit = null,
      AccountAmountInputModel? debit = null,
      AccountTransferInputModel? transfer = null)
  {
    var viewModel = await BuildDetailsViewModelAsync(id, cancellationToken).ConfigureAwait(false);
    if (viewModel is null)
    {
      return NotFound();
    }

    if (credit is not null)
    {
      viewModel.Credit.Amount = credit.Amount;
    }

    if (debit is not null)
    {
      viewModel.Debit.Amount = debit.Amount;
    }

    if (transfer is not null)
    {
      viewModel.Transfer.Amount = transfer.Amount;
      viewModel.Transfer.RecipientAccountId = transfer.RecipientAccountId;
    }

    return View("Details", viewModel);
  }
}
