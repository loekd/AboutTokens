using InventoryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApiPolicy")]
public class InventoryController(ILogger<InventoryController> logger) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Inventory> Get()
    {
        var inventory = Enumerable.Range(1, 8).Select(index => new Inventory
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Description = Summaries[Random.Shared.Next(Summaries.Length)],
            AmountInStock = Random.Shared.Next(0, 10000)
        }).ToArray();

        _logger.LogTrace("Returning {InventoryCount} inventory items", inventory.Length);
        return inventory;
    }

    private static readonly string[] Summaries =
    [
        "Door",
        "Window",
        "Plant",
        "Bottle",
        "Cup",
        "Book",
        "Pen",
        "Computer",
        "Plug",
        "Lamp",
        "Table",
        "Chair",
        "Bed",
        "House",
        "Curtain",
        "Couch",
        "Radio",
        "TV"
    ];

    private readonly ILogger<InventoryController> _logger = logger;
}