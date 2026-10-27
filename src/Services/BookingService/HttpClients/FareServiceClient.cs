using System.Net.Http.Json;
using BookingService.DTOs;
using Microsoft.Extensions.Logging;

namespace BookingService.HttpClients;

public class FareServiceClient : IFareServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FareServiceClient> _logger;

    public FareServiceClient(HttpClient httpClient, ILogger<FareServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FareValidationResultDto?> ValidateFareAsync(int fareId, int instanceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/fares/{fareId}/validate/{instanceId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FareValidationResultDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call FareService for fare {FareId}", fareId);
            return null;
        }
    }
}
