using System.Security.Claims;
using FareService.DTOs;
using FareService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FareService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaresController : ControllerBase
{
    private readonly IFareService _fareService;

    public FaresController(IFareService fareService)
    {
        _fareService = fareService;
    }

    [HttpGet("quote")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFareQuote(
        [FromQuery] int? flightId,
        [FromQuery] int? flightInstanceId,
        [FromQuery] string? cabin,
        [FromQuery] int? passengers)
    {
        var result = await _fareService.GetFareQuoteAsync(flightId, flightInstanceId, cabin ?? string.Empty, passengers ?? 1);
        return Ok(result);
    }

    [HttpGet("{flightInstanceId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByFlightInstance(int flightInstanceId)
    {
        var fares = await _fareService.GetActiveFaresAsync(flightInstanceId);
        return Ok(fares);
    }

    [HttpGet("details/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFareById(int id)
    {
        var fare = await _fareService.GetFareByIdAsync(id);
        return Ok(fare);
    }

    [HttpGet("{fareId:int}/validate/{instanceId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateFare(int fareId, int instanceId)
    {
        var fare = await _fareService.ValidateFareAsync(fareId, instanceId);
        return Ok(fare);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateFare([FromBody] CreateFareDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await _fareService.CreateFareAsync(dto, userId, isAdmin);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateFare(int id, [FromBody] UpdateFareDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await _fareService.UpdateFareAsync(id, dto, userId, isAdmin);
        return Ok(result);
    }

    [HttpGet("my-fares")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetMyFares()
    {
        var userId = GetCurrentUserId();
        var fares = await _fareService.GetMyFaresAsync(userId);
        return Ok(fares);
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
