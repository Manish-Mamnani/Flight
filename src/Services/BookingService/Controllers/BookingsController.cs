using System.Security.Claims;
using BookingService.DTOs;
using BookingService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [Authorize(Roles = "User,Admin,Manager")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _bookingService.CreateBookingAsync(dto, userId);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = GetCurrentUserId();
        var bookings = await _bookingService.GetMyBookingsAsync(userId);
        return Ok(bookings);
    }

    [HttpGet("{reference}")]
    [Authorize]
    public async Task<IActionResult> GetByReference(string reference)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var booking = await _bookingService.GetBookingAsync(reference, userId, isAdmin);
        return Ok(booking);
    }

    [HttpPost("{reference}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(string reference)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await _bookingService.CancelBookingAsync(reference, userId, isAdmin);
        return Ok(result);
    }

    [HttpGet("{reference}/passengers")]
    [Authorize]
    public async Task<IActionResult> GetPassengers(string reference)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var passengers = await _bookingService.GetPassengersAsync(reference, userId, isAdmin);
        return Ok(passengers);
    }

    [HttpGet("{reference}/validate")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateBooking(string reference)
    {
        var validation = await _bookingService.ValidateBookingForCheckInAsync(reference);
        return Ok(validation);
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

        throw new UnauthorizedAccessException("User is not authenticated.");
    }
}
