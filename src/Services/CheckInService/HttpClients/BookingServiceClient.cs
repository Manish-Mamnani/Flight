using System.Net.Http.Json;
using CheckInService.DTOs;
using Microsoft.Extensions.Logging;

namespace CheckInService.HttpClients;

public class BookingServiceClient : IBookingServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BookingServiceClient> _logger;

    public BookingServiceClient(HttpClient httpClient, ILogger<BookingServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BookingDetailsResponseDto?> GetBookingValidationAsync(string referenceOrId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/bookings/{referenceOrId}/validate");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BookingService returned {StatusCode} for reference {Reference}", response.StatusCode, referenceOrId);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<BookingDetailsResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with BookingService for reference {Reference}", referenceOrId);
            return null;
        }
    }
}
