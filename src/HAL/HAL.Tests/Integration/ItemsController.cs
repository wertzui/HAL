using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Controllers;
using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace HAL.Tests.Integration;

/// <summary>
/// A minimal controller used by integration tests to exercise real HAL serialisation
/// via a live in-process ASP.NET Core host.
/// </summary>
[Route("api/[controller]")]
public class ItemsController(IResourceFactory resourceFactory) : HalControllerBase
{
    private static readonly List<ItemDto> _seedItems =
    [
        new ItemDto(1, "Apple"),
        new ItemDto(2, "Banana"),
        new ItemDto(3, "Cherry"),
    ];

    // Scoped per DI scope (transient-like) so each test gets a fresh copy.
    private readonly List<ItemDto> _items = [.. _seedItems];

    [HttpGet]
    public ActionResult<Resource> GetList()
    {
        var resource = resourceFactory.CreateForListEndpoint(
            _items,
            _ => "items",
            i => i.Id,
            "Items");
        return Ok(resource);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Resource<ItemDto>> Get(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is null)
            return NotFound();

        var resource = resourceFactory.CreateForEndpoint(item, "Get", "Items", new { id });
        return Ok(resource);
    }

    [HttpPost]
    public ActionResult<Resource<ItemDto>> Post([FromBody] ItemDto item)
    {
        _items.Add(item);
        var resource = resourceFactory.CreateForEndpoint(item, "Get", "Items", new { id = item.Id });
        return CreatedAtAction(nameof(Get), new { id = item.Id }, resource);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is null)
            return NotFound();

        _items.Remove(item);
        return NoContent();
    }

    public record ItemDto(int Id, string Name);
}
