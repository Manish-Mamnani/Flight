using SearchService.DTOs;
using SearchService.Exceptions;
using SearchService.Models;
using SearchService.Repositories.Interfaces;
using SearchService.Services.Interfaces;

namespace SearchService.Services;

public class AirportService : IAirportService
{
    private readonly IAirportRepository _airportRepository;

    public AirportService(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
    }

    public async Task<IEnumerable<AirportDto>> GetAirportsAsync()
    {
        var airports = await _airportRepository.GetAllActiveAsync();
        return airports.Select(a => new AirportDto
        {
            AirportId = a.AirportId,
            AirportCode = a.AirportCode,
            AirportName = a.AirportName,
            City = a.City,
            Country = a.Country,
            Status = a.Status
        });
    }

    public async Task<AirportDto> CreateAirportAsync(CreateAirportDto dto)
    {
        var existing = await _airportRepository.GetByCodeAsync(dto.AirportCode);
        if (existing != null)
        {
            throw new DuplicateAirportCodeException($"Airport with code '{dto.AirportCode.ToUpper()}' already exists.");
        }

        var airport = new Airport
        {
            AirportCode = dto.AirportCode.Trim().ToUpperInvariant(),
            AirportName = dto.AirportName.Trim(),
            City = dto.City.Trim(),
            Country = dto.Country.Trim(),
            Status = "Active"
        };

        await _airportRepository.AddAsync(airport);
        await _airportRepository.SaveChangesAsync();

        return new AirportDto
        {
            AirportId = airport.AirportId,
            AirportCode = airport.AirportCode,
            AirportName = airport.AirportName,
            City = airport.City,
            Country = airport.Country,
            Status = airport.Status
        };
    }

    public async Task<AirportDto> UpdateAirportAsync(int id, UpdateAirportDto dto)
    {
        var airport = await _airportRepository.GetByIdAsync(id);
        if (airport == null)
        {
            throw new AirportNotFoundException($"Airport with ID {id} was not found.");
        }

        if (!string.IsNullOrWhiteSpace(dto.AirportName))
        {
            airport.AirportName = dto.AirportName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.City))
        {
            airport.City = dto.City.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Country))
        {
            airport.Country = dto.Country.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            airport.Status = dto.Status.Trim();
        }

        await _airportRepository.UpdateAsync(airport);
        await _airportRepository.SaveChangesAsync();

        return new AirportDto
        {
            AirportId = airport.AirportId,
            AirportCode = airport.AirportCode,
            AirportName = airport.AirportName,
            City = airport.City,
            Country = airport.Country,
            Status = airport.Status
        };
    }
}
