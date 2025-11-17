using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenesisCars.Application.Exceptions;
using GenesisCars.Application.Inventory;
using GenesisCars.Application.Marketplace;
using GenesisCars.Application.Payments;
using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Exceptions;
using GenesisCars.Web.Models.Marketplace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenesisCars.Web.Controllers;

[Authorize]
public class MarketplaceController : Controller
{
  private readonly IMarketplaceService _marketplaceService;
  private readonly ICarService _carService;
  private readonly IPaymentService _paymentService;

  public MarketplaceController(
      IMarketplaceService marketplaceService,
      ICarService carService,
      IPaymentService paymentService)
  {
    _marketplaceService = marketplaceService;
    _carService = carService;
    _paymentService = paymentService;
  }

  public async Task<IActionResult> Index(CancellationToken cancellationToken)
  {
    var listings = await _marketplaceService.GetAllAsync(cancellationToken).ConfigureAwait(false);
    return View(listings);
  }

  public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
  {
    var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (listing is null)
    {
      return NotFound();
    }

    if (TempData.TryGetValue("MarketplaceError", out var message))
    {
      ViewData["MarketplaceError"] = message;
    }

    var payments = await _paymentService.GetByListingIdAsync(listing.Id, cancellationToken).ConfigureAwait(false);
    ViewBag.Payments = payments;

    return View(listing);
  }

  public async Task<IActionResult> Create(CancellationToken cancellationToken)
  {
    await PopulateCarOptionsAsync(cancellationToken).ConfigureAwait(false);
    return View(new MarketplaceListingInputModel { AskingPrice = 1000m });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(MarketplaceListingInputModel input, CancellationToken cancellationToken)
  {
    if (input.CarId == Guid.Empty)
    {
      ModelState.AddModelError(nameof(input.CarId), "Please select a car.");
    }

    if (!ModelState.IsValid)
    {
      await PopulateCarOptionsAsync(cancellationToken).ConfigureAwait(false);
      return View(input);
    }

    try
    {
      var created = await _marketplaceService.CreateAsync(
          new CreateMarketplaceListingRequest(input.CarId, input.AskingPrice, input.Description),
          cancellationToken).ConfigureAwait(false);

      TempData["StatusMessage"] = "Marketplace listing created.";
      return RedirectToAction(nameof(Details), new { id = created.Id });
    }
    catch (NotFoundException ex)
    {
      ModelState.AddModelError(nameof(input.CarId), ex.Message);
    }
    catch (ConflictException ex)
    {
      ModelState.AddModelError(string.Empty, ex.Message);
    }
    catch (DomainException ex)
    {
      ModelState.AddModelError(string.Empty, ex.Message);
    }

    await PopulateCarOptionsAsync(cancellationToken).ConfigureAwait(false);
    return View(input);
  }

  public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
  {
    var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (listing is null)
    {
      return NotFound();
    }

    var model = new MarketplaceListingEditModel
    {
      AskingPrice = listing.AskingPrice,
      Description = listing.Description
    };

    ViewBag.Listing = listing;
    return View(model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(Guid id, MarketplaceListingEditModel input, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      var snapshot = await _marketplaceService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
      if (snapshot is null)
      {
        return NotFound();
      }

      ViewBag.Listing = snapshot;
      return View(input);
    }

    try
    {
      await _marketplaceService.UpdateAsync(
          id,
          new UpdateMarketplaceListingRequest(input.AskingPrice, input.Description),
          cancellationToken).ConfigureAwait(false);

      TempData["StatusMessage"] = "Marketplace listing updated.";
      return RedirectToAction(nameof(Details), new { id });
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
    catch (DomainException ex)
    {
      ModelState.AddModelError(string.Empty, ex.Message);
    }

    var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (listing is null)
    {
      return NotFound();
    }

    ViewBag.Listing = listing;
    return View(input);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> MarkAsSold(Guid id, CancellationToken cancellationToken)
  {
    try
    {
      await _marketplaceService.MarkAsSoldAsync(id, cancellationToken).ConfigureAwait(false);
      TempData["StatusMessage"] = "Listing marked as sold.";
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
    catch (DomainException ex)
    {
      TempData["MarketplaceError"] = ex.Message;
    }

    return RedirectToAction(nameof(Details), new { id });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
  {
    try
    {
      await _marketplaceService.ArchiveAsync(id, cancellationToken).ConfigureAwait(false);
      TempData["StatusMessage"] = "Listing archived.";
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
    catch (DomainException ex)
    {
      TempData["MarketplaceError"] = ex.Message;
    }

    return RedirectToAction(nameof(Details), new { id });
  }

  private async Task PopulateCarOptionsAsync(CancellationToken cancellationToken)
  {
    var cars = await _carService.GetAllAsync(cancellationToken).ConfigureAwait(false);
    var listings = await _marketplaceService.GetAllAsync(cancellationToken).ConfigureAwait(false);

    var unavailableCarIds = new HashSet<Guid>(
        listings
            .Where(listing => listing.Status == MarketplaceListingStatus.Active)
            .Select(listing => listing.Car.Id));

    var options = cars
        .Where(car => !unavailableCarIds.Contains(car.Id))
        .OrderByDescending(car => car.Year)
        .ThenBy(car => car.Model, StringComparer.OrdinalIgnoreCase)
        .Select(car => new SelectListItem
        {
          Text = $"{car.Year} {car.Model} ({car.Price:C})",
          Value = car.Id.ToString()
        })
        .ToList();

    ViewBag.CarOptions = options;
  }
}
