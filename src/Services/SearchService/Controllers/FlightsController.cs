using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.DTOs;
using SearchService.Services.Interfaces;

namespace SearchService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly ISearchService _searchService;

    public FlightsController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchFlights([FromQuery] string origin, [FromQuery] string destination, [FromQuery] DateTime date)
    {
        var flights = await _searchService.SearchAsync(origin, destination, date);
        return Ok(flights);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFlight(int id)
    {
        var flight = await _searchService.GetFlightAsync(id);
        return Ok(flight);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateFlight([FromBody] CreateFlightDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _searchService.CreateFlightAsync(dto, userId);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateFlight(int id, [FromBody] UpdateFlightDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await _searchService.UpdateFlightAsync(id, dto, userId, isAdmin);
        return Ok(result);
    }

    [HttpGet("my-flights")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetMyFlights()
    {
        var userId = GetCurrentUserId();
        var flights = await _searchService.GetMyFlightsAsync(userId);
        return Ok(flights);
    }

    [HttpGet("managed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetManagedFlights()
    {
        var flights = await _searchService.GetManagedFlightsAsync();
        return Ok(flights);
    }

    [HttpGet("instances/{instanceId:int}/validate")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateInstance(int instanceId)
    {
        var result = await _searchService.ValidateInstanceAsync(instanceId);
        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("UserId")?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (int.TryParse(claim, out var userId))
        {
            return userId;
        }

        return 0;
    }
}
