using System.Net.Http.Json;
using System.Text.Json;

namespace FxExpert.Blazor.Client.Services;

public class UserThemeService : IUserThemeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserThemeService> _logger;

    public UserThemeService(HttpClient httpClient, ILogger<UserThemeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GetUserThemeAsync(string emailAddress)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/theme/{Uri.EscapeDataString(emailAddress)}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ThemeResponse>();
                return result?.Theme;
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("User theme not found for {EmailAddress}, returning default", emailAddress);
                return null; // Will use default theme
            }
            
            _logger.LogWarning("Failed to get user theme for {EmailAddress}: {StatusCode}", emailAddress, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user theme for {EmailAddress}", emailAddress);
            return null;
        }
    }

    public async Task SetUserThemeAsync(string emailAddress, string theme)
    {
        try
        {
            var command = new UpdateUserThemeCommand(emailAddress, theme);
            var response = await _httpClient.PostAsJsonAsync($"api/users/theme/{Uri.EscapeDataString(emailAddress)}", command);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to set user theme for {EmailAddress} to {Theme}: {StatusCode}", 
                    emailAddress, theme, response.StatusCode);
            }
            else
            {
                _logger.LogInformation("Successfully updated theme to {Theme} for user {EmailAddress}", theme, emailAddress);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user theme for {EmailAddress} to {Theme}", emailAddress, theme);
        }
    }

    private record ThemeResponse(string Theme);
    private record UpdateUserThemeCommand(string EmailAddress, string Theme);
}