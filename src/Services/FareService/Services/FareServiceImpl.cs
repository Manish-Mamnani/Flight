using FareService.DTOs;
using FareService.Exceptions;
using FareService.HttpClients;
using FareService.Models;
using FareService.Repositories.Interfaces;
using FareService.Services.Interfaces;

namespace FareService.Services;

public class FareServiceImpl : IFareService
{
    private static readonly HashSet<string> AllowedClasses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Economy", "Business", "First"
    };

    private readonly IFareRepository _fareRepository;
    private readonly ISearchServiceClient _searchServiceClient;

    public FareServiceImpl(IFareRepository fareRepository, ISearchServiceClient searchServiceClient)
    {
        _fareRepository = fareRepository;
        _searchServiceClient = searchServiceClient;
    }

    public async Task<IEnumerable<FareDto>> GetActiveFaresAsync(int flightInstanceId)
    {
        var fares = await _fareRepository.GetByFlightInstanceIdAsync(flightInstanceId);
        return fares.Select(MapToFareDto);
    }

    public async Task<FareDto> GetFareByIdAsync(int fareId)
    {
        var fare = await _fareRepository.GetByIdAsync(fareId);
        if (fare == null)
        {
            throw new FareNotFoundException($"Fare with ID {fareId} was not found.");
        }

        return MapToFareDto(fare);
    }

    public async Task<FareDto> CreateFareAsync(CreateFareDto dto, int userId, bool isAdmin)
    {
        if (dto.BaseFare < 0 || dto.TaxAmount < 0)
        {
            throw new InvalidFareOperationException("BaseFare and TaxAmount must be non-negative.");
        }

        if (!AllowedClasses.Contains(dto.FareClass.Trim()))
        {
            throw new InvalidFareOperationException($"Invalid fare class: {dto.FareClass}. Allowed classes: Economy, Business, First.");
        }

        if (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3)
        {
            throw new InvalidFareOperationException("Currency code must be a 3-letter code.");
        }

        // Validate instance exists and manager ownership against SearchService
        var flightInstance = await _searchServiceClient.ValidateFlightInstanceAsync(dto.FlightInstanceId);
        if (flightInstance != null)
        {
            if (!isAdmin && flightInstance.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You can only manage fares for flights you own.");
            }
        }

        var fare = new Fare
        {
            FlightInstanceId = dto.FlightInstanceId,
            FareClass = dto.FareClass.Trim(),
            BaseFare = dto.BaseFare,
            TaxAmount = dto.TaxAmount,
            CurrencyCode = dto.CurrencyCode.Trim().ToUpperInvariant(),
            Status = "Active"
        };

        await _fareRepository.AddAsync(fare);
        await _fareRepository.SaveChangesAsync();

        return MapToFareDto(fare);
    }

    public async Task<FareDto> UpdateFareAsync(int fareId, UpdateFareDto dto, int userId, bool isAdmin)
    {
        var fare = await _fareRepository.GetByIdAsync(fareId);
        if (fare == null)
        {
            throw new FareNotFoundException($"Fare with ID {fareId} was not found.");
        }

        if (dto.BaseFare.HasValue)
        {
            if (dto.BaseFare.Value < 0)
            {
                throw new InvalidFareOperationException("BaseFare must be non-negative.");
            }
            fare.BaseFare = dto.BaseFare.Value;
        }

        if (dto.TaxAmount.HasValue)
        {
            if (dto.TaxAmount.Value < 0)
            {
                throw new InvalidFareOperationException("TaxAmount must be non-negative.");
            }
            fare.TaxAmount = dto.TaxAmount.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.CurrencyCode))
        {
            if (dto.CurrencyCode.Trim().Length != 3)
            {
                throw new InvalidFareOperationException("Currency code must be a 3-letter code.");
            }
            fare.CurrencyCode = dto.CurrencyCode.Trim().ToUpperInvariant();
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            fare.Status = dto.Status.Trim();
        }

        await _fareRepository.UpdateAsync(fare);
        await _fareRepository.SaveChangesAsync();

        return MapToFareDto(fare);
    }

    public async Task<IEnumerable<FareDto>> GetMyFaresAsync(int managerId)
    {
        // Return active fares
        var fares = await _fareRepository.GetByFlightInstanceIdAsync(0);
        return fares.Select(MapToFareDto);
    }

    public async Task<FareQuoteResponseDto> GetFareQuoteAsync(int? flightId, int? flightInstanceId, string cabin, int passengers)
    {
        if (flightId == null && flightInstanceId == null)
        {
            throw new ArgumentException("flightId is required");
        }

        if (passengers <= 0)
        {
            throw new ArgumentException("at least one passenger required");
        }

        if (string.IsNullOrWhiteSpace(cabin) || !AllowedClasses.Contains(cabin.Trim()))
        {
            throw new ArgumentException("invalid cabin class");
        }

        var targetInstanceId = flightInstanceId ?? flightId!.Value;
        var fare = await _fareRepository.GetByInstanceAndClassAsync(targetInstanceId, cabin.Trim());

        if (fare == null)
        {
            // If checking by flightId, check if any fare exists for this class
            var activeFares = await _fareRepository.GetByFlightInstanceIdAsync(targetInstanceId);
            fare = activeFares.FirstOrDefault(f => f.FareClass.Equals(cabin.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (fare == null)
        {
            throw new FareNotFoundException("fare not available");
        }

        var perPassenger = fare.BaseFare + fare.TaxAmount;
        var totalAmount = perPassenger * passengers;

        return new FareQuoteResponseDto
        {
            FareId = fare.FareId,
            FlightId = flightId ?? fare.FlightInstanceId,
            FlightInstanceId = fare.FlightInstanceId,
            FareClass = fare.FareClass,
            PerPassengerFare = perPassenger,
            PassengerCount = passengers,
            TotalAmount = totalAmount,
            Currency = fare.CurrencyCode
        };
    }

    public async Task<FareDto> ValidateFareAsync(int fareId, int flightInstanceId)
    {
        var fare = await _fareRepository.GetByIdAsync(fareId);
        if (fare == null || fare.Status != "Active")
        {
            throw new FareNotFoundException("Fare is not active or does not exist.");
        }

        if (fare.FlightInstanceId != flightInstanceId)
        {
            throw new InvalidFareOperationException("Fare does not match the requested flight instance.");
        }

        return MapToFareDto(fare);
    }

    private static FareDto MapToFareDto(Fare fare)
    {
        return new FareDto
        {
            FareId = fare.FareId,
            FlightInstanceId = fare.FlightInstanceId,
            FareClass = fare.FareClass,
            BaseFare = fare.BaseFare,
            TaxAmount = fare.TaxAmount,
            CurrencyCode = fare.CurrencyCode,
            Status = fare.Status
        };
    }
}
