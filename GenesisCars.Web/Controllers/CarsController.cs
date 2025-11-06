using GenesisCars.Application.Exceptions;
using GenesisCars.Application.Inventory;
using GenesisCars.Web.Models.Cars;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenesisCars.Web.Controllers;

[Authorize]
public class CarsController : Controller
{
  private readonly ICarService _carService;

  public CarsController(ICarService carService)
  {
    _carService = carService;
  }

  public async Task<IActionResult> Index(CancellationToken cancellationToken)
  {
    var cars = await _carService.GetAllAsync(cancellationToken);
    return View(cars);
  }

  public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
  {
    var car = await _carService.GetByIdAsync(id, cancellationToken);
    if (car is null)
    {
      return NotFound();
    }

    return View(car);
  }

  public IActionResult Create()
  {
    return View(new CarInputModel { Year = DateTime.UtcNow.Year });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(CarInputModel input, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return View(input);
    }

    var created = await _carService.CreateAsync(
        new CreateCarRequest(input.Model, input.Year, input.Price),
        cancellationToken);

    return RedirectToAction(nameof(Details), new { id = created.Id });
  }

  public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
  {
    var car = await _carService.GetByIdAsync(id, cancellationToken);
    if (car is null)
    {
      return NotFound();
    }

    var model = new CarInputModel
    {
      Model = car.Model,
      Year = car.Year,
      Price = car.Price
    };

    return View(model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(Guid id, CarInputModel input, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      return View(input);
    }

    try
    {
      await _carService.UpdateAsync(
          id,
          new UpdateCarRequest(input.Model, input.Year, input.Price),
          cancellationToken);

      return RedirectToAction(nameof(Details), new { id });
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
  }

  public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
  {
    var car = await _carService.GetByIdAsync(id, cancellationToken);
    if (car is null)
    {
      return NotFound();
    }

    return View(car);
  }

  [HttpPost, ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
  {
    try
    {
      await _carService.DeleteAsync(id, cancellationToken);
      return RedirectToAction(nameof(Index));
    }
    catch (NotFoundException)
    {
      return NotFound();
    }
  }
}
