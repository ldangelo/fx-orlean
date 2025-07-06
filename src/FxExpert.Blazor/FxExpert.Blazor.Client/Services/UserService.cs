using System.Net.Http.Json;
using Fortium.Types;

namespace FxExpert.Blazor.Client.Services;

public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<User?> GetUserProfileAsync(string emailAddress)
    {
        if (string.IsNullOrEmpty(emailAddress))
        {
            Console.WriteLine("Email address is null or empty");
            return null;
        }
        
        try
        {
            return await _httpClient.GetFromJsonAsync<User>($"api/users/{emailAddress}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching user profile: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateUserProfileAsync(string emailAddress, string? firstName, string? lastName, string? phoneNumber, string? profilePictureUrl)
    {
        if (string.IsNullOrEmpty(emailAddress))
        {
            Console.WriteLine("Email address is null or empty");
            return false;
        }
        
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/users/profile/{emailAddress}", new
            {
                EmailAddress = emailAddress,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                ProfilePictureUrl = profilePictureUrl
            });

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user profile: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateUserAddressAsync(
        string emailAddress, 
        string? street1, 
        string? street2,
        string? city,
        string? state,
        string? zipCode,
        string? country)
    {
        if (string.IsNullOrEmpty(emailAddress))
        {
            Console.WriteLine("Email address is null or empty");
            return false;
        }
        
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/users/address/{emailAddress}", new
            {
                EmailAddress = emailAddress,
                Street1 = street1,
                Street2 = street2,
                City = city,
                State = state,
                ZipCode = zipCode,
                Country = country
            });

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user address: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateUserPreferencesAsync(
        string emailAddress,
        bool receiveEmailNotifications,
        bool receiveSmsNotifications,
        string? preferredLanguage,
        string? timeZone,
        string? theme)
    {
        if (string.IsNullOrEmpty(emailAddress))
        {
            Console.WriteLine("Email address is null or empty");
            return false;
        }
        
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/users/preferences/{emailAddress}", new
            {
                EmailAddress = emailAddress,
                ReceiveEmailNotifications = receiveEmailNotifications,
                ReceiveSmsNotifications = receiveSmsNotifications,
                PreferredLanguage = preferredLanguage,
                TimeZone = timeZone,
                Theme = theme
            });

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user preferences: {ex.Message}");
            return false;
        }
    }
}