using SearchService.DTOs;
using SearchService.Exceptions;
using SearchService.Models;
using SearchService.Repositories.Interfaces;
using SearchService.Services.Interfaces;

namespace SearchService.Services;

public class FlightSearchService : ISearchService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IFlightInstanceRepository _flightInstanceRepository;
    private readonly IAirportRepository _airportRepository;
    private readonly IAirlineRepository _airlineRepository;

    public FlightSearchService(
        IFlightRepository flightRepository,
        IFlightInstanceRepository flightInstanceRepository,
        IAirportRepository airportRepository,
        IAirlineRepository airlineRepository)
    {
        _flightRepository = flightRepository;
        _flightInstanceRepository = flightInstanceRepository;
        _airportRepository = airportRepository;
        _airlineRepository = airlineRepository;
    }

    public async Task<IEnumerable<FlightResponseDto>> SearchAsync(string origin, string destination, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new ArgumentException("origin is required");
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentException("destination is required");
        }

        if (origin.Trim().Equals(destination.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("origin and destination must differ");
        }

        if (date.Date < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("date cannot be in the past");
        }

        var flights = await _flightRepository.SearchFlightsAsync(origin, destination, date);
        return flights.Select(MapToFlightResponseDto);
    }

    public async Task<FlightResponseDto> GetFlightAsync(int flightId)
    {
        var flight = await _flightRepository.GetByIdWithDetailsAsync(flightId);
        if (flight == null)
        {
            throw new FlightNotFoundException($"Flight with ID {flightId} was not found.");
        }

        return MapToFlightResponseDto(flight);
    }

    public async Task<FlightResponseDto> CreateFlightAsync(CreateFlightDto dto, int createdByUserId)
    {
        if (dto.OriginAirportId == dto.DestinationAirportId)
        {
            throw new InvalidFlightOperationException("Origin and destination airports must differ.");
        }

        var origin = await _airportRepository.GetByIdAsync(dto.OriginAirportId);
        var destination = await _airportRepository.GetByIdAsync(dto.DestinationAirportId);
        var airline = await _airlineRepository.GetByIdAsync(dto.AirlineId);

        if (origin == null || destination == null)
        {
            throw new AirportNotFoundException("Origin or destination airport does not exist.");
        }

        if (airline == null)
        {
            throw new InvalidFlightOperationException("Airline does not exist.");
        }

        var flight = new Flight
        {
            FlightNumber = dto.FlightNumber.Trim().ToUpperInvariant(),
            AirlineId = dto.AirlineId,
            OriginAirportId = dto.OriginAirportId,
            DestinationAirportId = dto.DestinationAirportId,
            CreatedByUserId = createdByUserId,
            Status = "Active"
        };

        if (dto.Instances != null && dto.Instances.Any())
        {
            foreach (var inst in dto.Instances)
            {
                if (inst.ArrivalAtUtc <= inst.DepartureAtUtc)
                {
                    throw new InvalidFlightOperationException("Arrival time must be later than departure time.");
                }

                if (inst.TotalSeats <= 0)
                {
                    throw new InvalidFlightOperationException("Total seats must be greater than 0.");
                }

                flight.FlightInstances.Add(new FlightInstance
                {
                    FlightId = flight.FlightId,
                    DepartureAtUtc = inst.DepartureAtUtc,
                    ArrivalAtUtc = inst.ArrivalAtUtc,
                    TotalSeats = inst.TotalSeats,
                    FlightStatus = "Scheduled"
                });
            }
        }

        await _flightRepository.AddAsync(flight);
        await _flightRepository.SaveChangesAsync();

        return await GetFlightAsync(flight.FlightId);
    }

    public async Task<FlightResponseDto> UpdateFlightAsync(int flightId, UpdateFlightDto dto, int userId, bool isAdmin)
    {
        var flight = await _flightRepository.GetByIdAsync(flightId);
        if (flight == null)
        {
            throw new FlightNotFoundException($"Flight with ID {flightId} was not found.");
        }

        if (!isAdmin && flight.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this flight.");
        }

        if (!string.IsNullOrWhiteSpace(dto.FlightNumber))
        {
            flight.FlightNumber = dto.FlightNumber.Trim().ToUpperInvariant();
        }

        if (dto.AirlineId.HasValue)
        {
            flight.AirlineId = dto.AirlineId.Value;
        }

        if (dto.OriginAirportId.HasValue)
        {
            flight.OriginAirportId = dto.OriginAirportId.Value;
        }

        if (dto.DestinationAirportId.HasValue)
        {
            flight.DestinationAirportId = dto.DestinationAirportId.Value;
        }

        if (flight.OriginAirportId == flight.DestinationAirportId)
        {
            throw new InvalidFlightOperationException("Origin and destination airports must differ.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            flight.Status = dto.Status.Trim();
        }

        await _flightRepository.UpdateAsync(flight);
        await _flightRepository.SaveChangesAsync();

        return await GetFlightAsync(flightId);
    }

    public async Task<IEnumerable<FlightResponseDto>> GetMyFlightsAsync(int managerId)
    {
        var flights = await _flightRepository.GetByManagerIdAsync(managerId);
        return flights.Select(MapToFlightResponseDto);
    }

    public async Task<IEnumerable<FlightResponseDto>> GetManagedFlightsAsync()
    {
        var flights = await _flightRepository.GetAllManagedAsync();
        return flights.Select(MapToFlightResponseDto);
    }

    public async Task<FlightValidationDto> ValidateInstanceAsync(int instanceId)
    {
        var instance = await _flightInstanceRepository.GetByIdWithFlightAsync(instanceId);
        if (instance == null)
        {
            throw new FlightNotFoundException($"Flight instance {instanceId} not found.");
        }

        return new FlightValidationDto
        {
            FlightInstanceId = instance.FlightInstanceId,
            FlightId = instance.FlightId,
            FlightNumber = instance.Flight.FlightNumber,
            OriginCode = instance.Flight.OriginAirport.AirportCode,
            DestinationCode = instance.Flight.DestinationAirport.AirportCode,
            DepartureAtUtc = instance.DepartureAtUtc,
            ArrivalAtUtc = instance.ArrivalAtUtc,
            TotalSeats = instance.TotalSeats,
            FlightStatus = instance.FlightStatus,
            CreatedByUserId = instance.Flight.CreatedByUserId,
            IsActive = instance.Flight.Status == "Active" && instance.FlightStatus != "Cancelled"
        };
    }

    private static FlightResponseDto MapToFlightResponseDto(Flight flight)
    {
        return new FlightResponseDto
        {
            FlightId = flight.FlightId,
            FlightNumber = flight.FlightNumber,
            AirlineCode = flight.Airline?.AirlineCode ?? string.Empty,
            AirlineName = flight.Airline?.AirlineName ?? string.Empty,
            OriginAirportCode = flight.OriginAirport?.AirportCode ?? string.Empty,
            OriginAirportName = flight.OriginAirport?.AirportName ?? string.Empty,
            OriginCity = flight.OriginAirport?.City ?? string.Empty,
            DestinationAirportCode = flight.DestinationAirport?.AirportCode ?? string.Empty,
            DestinationAirportName = flight.DestinationAirport?.AirportName ?? string.Empty,
            DestinationCity = flight.DestinationAirport?.City ?? string.Empty,
            CreatedByUserId = flight.CreatedByUserId,
            Status = flight.Status,
            Instances = flight.FlightInstances.Select(fi => new FlightInstanceDto
            {
                FlightInstanceId = fi.FlightInstanceId,
                FlightId = fi.FlightId,
                DepartureAtUtc = fi.DepartureAtUtc,
                ArrivalAtUtc = fi.ArrivalAtUtc,
                TotalSeats = fi.TotalSeats,
                FlightStatus = fi.FlightStatus
            }).ToList()
        };
    }
}
