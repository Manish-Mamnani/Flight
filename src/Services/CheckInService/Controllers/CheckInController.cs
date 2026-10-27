using System.Security.Claims;
using CheckInService.DTOs;
using CheckInService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckInService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    [HttpGet("{reference}/seats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSeats(string reference)
    {
        var seats = await _checkInService.GetAvailableSeatsAsync(reference);
        return Ok(seats);
    }

    [HttpPost("{reference}")]
    [Authorize]
    public async Task<IActionResult> CheckIn(string reference, [FromBody] CheckInRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await _checkInService.CheckInAsync(reference, request, userId, isAdmin);
        return StatusCode(StatusCodes.Status200OK, result);
    }

    [HttpPost("/api/checkins")]
    [AllowAnonymous]
    public async Task<IActionResult> DirectCheckIn([FromBody] CheckInRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _checkInService.CheckInAsync(request);
        return StatusCode(StatusCodes.Status200OK, result);
    }

    [HttpGet("{reference}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCheckInStatus(string reference)
    {
        var result = await _checkInService.GetCheckInStatusAsync(reference);
        return Ok(result);
    }

    [HttpPost("release/{bookingId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ReleaseSeats(int bookingId)
    {
        var result = await _checkInService.ReleaseSeatsForBookingAsync(bookingId);
        return Ok(new { released = result });
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
