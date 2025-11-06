using System.Security.Claims;
using GenesisCars.Application.Auth;
using GenesisCars.Application.Exceptions;
using GenesisCars.Domain.Exceptions;
using GenesisCars.Web.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenesisCars.Web.Controllers;

public class AccountController : Controller
{
  private readonly IAuthService _authService;

  public AccountController(IAuthService authService)
  {
    _authService = authService;
  }

  [AllowAnonymous]
  [HttpGet]
  public IActionResult Login(string? returnUrl = null)
  {
    if (User.Identity?.IsAuthenticated ?? false)
    {
      return RedirectToLocal(returnUrl);
    }

    return View(new LoginInputModel { ReturnUrl = returnUrl });
  }

  [AllowAnonymous]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Login(LoginInputModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return View(model);
    }

    var result = await _authService.AuthenticateAsync(
        new LoginRequest(model.Email, model.LastName),
        cancellationToken);

    if (result is null)
    {
      ModelState.AddModelError(string.Empty, "Invalid credentials.");
      return View(model);
    }

    await SignInAsync(result);

    return RedirectToLocal(model.ReturnUrl);
  }

  [AllowAnonymous]
  [HttpGet]
  public IActionResult Register(string? returnUrl = null)
  {
    if (User.Identity?.IsAuthenticated ?? false)
    {
      return RedirectToLocal(returnUrl);
    }

    return View(new RegisterInputModel { ReturnUrl = returnUrl });
  }

  [AllowAnonymous]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Register(RegisterInputModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return View(model);
    }

    try
    {
      var user = await _authService.RegisterAsync(
          new RegisterRequest(model.FirstName, model.LastName, model.Email),
          cancellationToken);

      await SignInAsync(user);

      return RedirectToLocal(model.ReturnUrl);
    }
    catch (ConflictException ex)
    {
      ModelState.AddModelError(nameof(model.Email), ex.Message);
      return View(model);
    }
    catch (DomainException ex)
    {
      ModelState.AddModelError(string.Empty, ex.Message);
      return View(model);
    }
  }

  [Authorize]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Logout()
  {
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Index", "Home");
  }

  private IActionResult RedirectToLocal(string? returnUrl)
  {
    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
    {
      return Redirect(returnUrl);
    }

    return RedirectToAction("Index", "Dashboard");
  }

  private async Task SignInAsync(AuthenticatedUserDto user)
  {
    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
      new Claim(ClaimTypes.Email, user.Email)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
  }
}
