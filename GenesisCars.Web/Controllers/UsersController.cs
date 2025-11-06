using GenesisCars.Application.Exceptions;
using GenesisCars.Application.Users;
using GenesisCars.Web.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenesisCars.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
  private readonly IUserService _userService;

  public UsersController(IUserService userService)
  {
    _userService = userService;
  }

  public async Task<IActionResult> Index(CancellationToken cancellationToken)
  {
    var users = await _userService.GetAllAsync(cancellationToken);
    return View(users);
  }

  public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
  {
    var user = await _userService.GetByIdAsync(id, cancellationToken);
    if (user is null)
    {
      return NotFound();
    }

    return View(user);
  }

  public IActionResult Create()
  {
    return View(new UserInputModel());
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(UserInputModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return View(model);
    }

    try
    {
      var created = await _userService.CreateAsync(
          new CreateUserRequest(model.FirstName, model.LastName, model.Email),
          cancellationToken);

      return RedirectToAction(nameof(Details), new { id = created.Id });
    }
    catch (ConflictException ex)
    {
      ModelState.AddModelError(nameof(model.Email), ex.Message);
      return View(model);
    }
  }

  public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
  {
    var user = await _userService.GetByIdAsync(id, cancellationToken);
    if (user is null)
    {
      return NotFound();
    }

    var model = new UserInputModel
    {
      FirstName = user.FirstName,
      LastName = user.LastName,
      Email = user.Email
    };

    return View(model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(Guid id, UserInputModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return View(model);
    }

    try
    {
      await _userService.UpdateAsync(
          id,
          new UpdateUserRequest(model.FirstName, model.LastName, model.Email),
          cancellationToken);

      return RedirectToAction(nameof(Details), new { id });
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
    catch (ConflictException ex)
    {
      ModelState.AddModelError(nameof(model.Email), ex.Message);
      return View(model);
    }
  }

  public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
  {
    var user = await _userService.GetByIdAsync(id, cancellationToken);
    if (user is null)
    {
      return NotFound();
    }

    return View(user);
  }

  [HttpPost, ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
  {
    try
    {
      await _userService.DeleteAsync(id, cancellationToken);
      return RedirectToAction(nameof(Index));
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
  }
}
