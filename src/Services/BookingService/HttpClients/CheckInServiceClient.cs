using Microsoft.Extensions.Logging;

namespace BookingService.HttpClients;

public class CheckInServiceClient : ICheckInServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CheckInServiceClient> _logger;

    public CheckInServiceClient(HttpClient httpClient, ILogger<CheckInServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ReleaseSeatsAsync(int bookingId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/checkin/release/{bookingId}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call CheckInService to release seats for booking {BookingId}", bookingId);
            return false;
        }
    }
}
