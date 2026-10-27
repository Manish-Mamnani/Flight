using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.DTOs;
using SearchService.Services.Interfaces;

namespace SearchService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AirportsController : ControllerBase
{
    private readonly IAirportService _airportService;

    public AirportsController(IAirportService airportService)
    {
        _airportService = airportService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAirports()
    {
        var airports = await _airportService.GetAirportsAsync();
        return Ok(airports);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAirport([FromBody] CreateAirportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _airportService.CreateAirportAsync(dto);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAirport(int id, [FromBody] UpdateAirportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _airportService.UpdateAirportAsync(id, dto);
        return Ok(result);
    }
}
