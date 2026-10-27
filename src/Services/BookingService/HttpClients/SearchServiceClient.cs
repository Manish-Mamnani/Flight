using System.Net.Http.Json;
using BookingService.DTOs;
using Microsoft.Extensions.Logging;

namespace BookingService.HttpClients;

public class SearchServiceClient : ISearchServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SearchServiceClient> _logger;

    public SearchServiceClient(HttpClient httpClient, ILogger<SearchServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FlightValidationResultDto?> ValidateFlightInstanceAsync(int instanceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/flights/instances/{instanceId}/validate");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FlightValidationResultDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call SearchService for flight instance {InstanceId}", instanceId);
            return null;
        }
    }
}
