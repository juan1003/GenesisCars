namespace GenesisCars.Application.Inventory;

public sealed record CreateCarRequest(string Model, int Year, decimal Price);
