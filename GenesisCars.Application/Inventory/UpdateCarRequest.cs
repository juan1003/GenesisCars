namespace GenesisCars.Application.Inventory;

public sealed record UpdateCarRequest(string Model, int Year, decimal Price);
