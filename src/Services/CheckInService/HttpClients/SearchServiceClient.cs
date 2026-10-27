using System.Net.Http.Json;
using CheckInService.DTOs;
using Microsoft.Extensions.Logging;

namespace CheckInService.HttpClients;

public class SearchServiceClient : ISearchServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SearchServiceClient> _logger;

    public SearchServiceClient(HttpClient httpClient, ILogger<SearchServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FlightDetailsResponseDto?> GetFlightInstanceAsync(int instanceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/flights/instances/{instanceId}/validate");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SearchService returned {StatusCode} for instance {InstanceId}", response.StatusCode, instanceId);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FlightDetailsResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with SearchService for instance {InstanceId}", instanceId);
            return null;
        }
    }
}
