using BookingService.DTOs;

namespace BookingService.HttpClients;

public interface IFareServiceClient
{
    Task<FareValidationResultDto?> ValidateFareAsync(int fareId, int instanceId);
}
