namespace InventoryApi.Models;

public class Inventory
{
    public DateOnly Date { get; set; }

    public int AmountInStock { get; set; }

    public decimal Price => Random.Shared.Next(1, 100) / 100m;

    public string? Description { get; set; }
}