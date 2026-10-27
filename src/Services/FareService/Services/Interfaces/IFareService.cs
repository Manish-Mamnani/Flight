using FareService.DTOs;

namespace FareService.Services.Interfaces;

public interface IFareService
{
    Task<IEnumerable<FareDto>> GetActiveFaresAsync(int flightInstanceId);
    Task<FareDto> GetFareByIdAsync(int fareId);
    Task<FareDto> CreateFareAsync(CreateFareDto dto, int userId, bool isAdmin);
    Task<FareDto> UpdateFareAsync(int fareId, UpdateFareDto dto, int userId, bool isAdmin);
    Task<IEnumerable<FareDto>> GetMyFaresAsync(int managerId);
    Task<FareQuoteResponseDto> GetFareQuoteAsync(int? flightId, int? flightInstanceId, string cabin, int passengers);
    Task<FareDto> ValidateFareAsync(int fareId, int flightInstanceId);
}
