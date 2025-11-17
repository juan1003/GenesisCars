using System;
using GenesisCars.Application.Exceptions;
using GenesisCars.Application.Marketplace;
using GenesisCars.Application.Payments;
using GenesisCars.Domain.Exceptions;
using GenesisCars.Web.Models.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenesisCars.Web.Controllers;

[Authorize]
public class PaymentsController : Controller
{
  private readonly IPaymentService _paymentService;
  private readonly IMarketplaceService _marketplaceService;

  public PaymentsController(IPaymentService paymentService, IMarketplaceService marketplaceService)
  {
    _paymentService = paymentService;
    _marketplaceService = marketplaceService;
  }

  [HttpGet]
  public async Task<IActionResult> Create(Guid listingId, CancellationToken cancellationToken)
  {
    if (listingId == Guid.Empty)
    {
      return BadRequest();
    }

    var listing = await _marketplaceService.GetByIdAsync(listingId, cancellationToken).ConfigureAwait(false);
    if (listing is null)
    {
      return NotFound();
    }

    if (listing.Status != GenesisCars.Domain.Entities.MarketplaceListingStatus.Active)
    {
      TempData["StatusMessage"] = "Listing is not available for payments.";
      return RedirectToAction("Details", "Marketplace", new { id = listingId });
    }

    var model = new PaymentIntentCreateModel
    {
      ListingId = listingId,
      Currency = "USD"
    };

    ViewBag.Listing = listing;
    return View(model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(PaymentIntentCreateModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      var listing = await _marketplaceService.GetByIdAsync(model.ListingId, cancellationToken).ConfigureAwait(false);
      if (listing is null)
      {
        return NotFound();
      }

      ViewBag.Listing = listing;
      return View(model);
    }

    try
    {
      var paymentIntent = await _paymentService.CreateAsync(new CreatePaymentIntentRequest(model.ListingId, model.Currency), cancellationToken).ConfigureAwait(false);
      TempData["StatusMessage"] = "Payment intent created.";
      return RedirectToAction(nameof(Details), new { id = paymentIntent.Id });
    }
    catch (NotFoundException ex)
    {
      ModelState.AddModelError(string.Empty, ex.Message);
    }
    catch (ConflictException ex)
    {
      ModelState.AddModelError(string.Empty, ex.Message);
    }
    catch (DomainException ex)
    {
      ModelState.AddModelError(string.Empty, ex.Message);
    }

    var fallbackListing = await _marketplaceService.GetByIdAsync(model.ListingId, cancellationToken).ConfigureAwait(false);
    if (fallbackListing is null)
    {
      return NotFound();
    }

    ViewBag.Listing = fallbackListing;
    return View(model);
  }

  [HttpGet]
  public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
  {
    var paymentIntent = await _paymentService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (paymentIntent is null)
    {
      return NotFound();
    }

    var listing = await _marketplaceService.GetByIdAsync(paymentIntent.ListingId, cancellationToken).ConfigureAwait(false);
    if (listing is null)
    {
      return NotFound();
    }

    var viewModel = new PaymentIntentViewModel
    {
      Listing = listing,
      PaymentIntent = paymentIntent
    };

    return View(viewModel);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
  {
    try
    {
      await _paymentService.ConfirmAsync(id, cancellationToken).ConfigureAwait(false);
      TempData["StatusMessage"] = "Payment confirmed.";
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
    catch (ConflictException ex)
    {
      TempData["StatusMessage"] = ex.Message;
    }
    catch (DomainException ex)
    {
      TempData["StatusMessage"] = ex.Message;
    }

    return RedirectToAction(nameof(Details), new { id });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
  {
    try
    {
      await _paymentService.CancelAsync(id, cancellationToken).ConfigureAwait(false);
      TempData["StatusMessage"] = "Payment canceled.";
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
    catch (ConflictException ex)
    {
      TempData["StatusMessage"] = ex.Message;
    }
    catch (DomainException ex)
    {
      TempData["StatusMessage"] = ex.Message;
    }

    return RedirectToAction(nameof(Details), new { id });
  }
}
