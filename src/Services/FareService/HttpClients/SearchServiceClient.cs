using System.Net.Http.Json;
using FareService.DTOs;
using Microsoft.Extensions.Logging;

namespace FareService.HttpClients;

public class SearchServiceClient : ISearchServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SearchServiceClient> _logger;

    public SearchServiceClient(HttpClient httpClient, ILogger<SearchServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FlightValidationResponseDto?> ValidateFlightInstanceAsync(int instanceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/flights/instances/{instanceId}/validate");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SearchService responded with status {StatusCode} for instance {InstanceId}", response.StatusCode, instanceId);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FlightValidationResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call SearchService for flight instance {InstanceId}", instanceId);
            return null;
        }
    }
}
