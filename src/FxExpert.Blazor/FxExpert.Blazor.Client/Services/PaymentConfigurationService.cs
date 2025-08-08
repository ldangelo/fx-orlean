using System.Text.Json;

namespace FxExpert.Blazor.Client.Services;

public class PaymentConfigurationService : IPaymentConfigurationService
{
    private readonly HttpClient _httpClient;
    private string? _cachedPublishableKey;

    public PaymentConfigurationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetStripePublishableKeyAsync()
    {
        if (!string.IsNullOrEmpty(_cachedPublishableKey))
        {
            return _cachedPublishableKey;
        }

        try
        {
            // Call the server to get the publishable key
            var response = await _httpClient.GetAsync("/api/payment/config/publishable-key");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var configResponse = JsonSerializer.Deserialize<PublishableKeyResponse>(
                    content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                
                _cachedPublishableKey = configResponse?.PublishableKey ?? "";
                return _cachedPublishableKey;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting Stripe publishable key: {ex.Message}");
        }

        // Fallback - this should be replaced with actual configuration
        return Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY") ?? "";
    }

    private record PublishableKeyResponse(string PublishableKey);
}