using System.Net.Http.Json;
using System.Net;
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
            return await _httpClient.GetFromJsonAsync<User>($"users/{emailAddress}");
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
            Console.WriteLine($"🔄 Updating user profile for: {emailAddress}");
            Console.WriteLine($"   Base URL: {_httpClient.BaseAddress}");
            Console.WriteLine($"   Full URL: {_httpClient.BaseAddress}users/profile/{emailAddress}");
            
            var requestData = new
            {
                EmailAddress = emailAddress,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                ProfilePictureUrl = profilePictureUrl
            };
            
            Console.WriteLine($"   Request data: {System.Text.Json.JsonSerializer.Serialize(requestData)}");
            
            var response = await _httpClient.PostAsJsonAsync($"users/profile/{emailAddress}", requestData);
            
            Console.WriteLine($"   Response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"   Error response: {errorContent}");
                throw new Exception($"HTTP {response.StatusCode}: {errorContent}");
            }

            Console.WriteLine("✅ Profile update successful");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating user profile: {ex.Message}");
            Console.WriteLine($"   Exception type: {ex.GetType().Name}");
            Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            throw; // Re-throw to let the UI handle the detailed error
        }
    }

    public async Task<bool> CreateUserAsync(string emailAddress, string firstName, string lastName)
    {
        if (string.IsNullOrEmpty(emailAddress))
        {
            Console.WriteLine("Email address is null or empty");
            return false;
        }
        
        try
        {
            Console.WriteLine($"🆕 Creating new user: {emailAddress}");
            
            var requestData = new
            {
                EmailAddress = emailAddress,
                FirstName = firstName ?? "Unknown",
                LastName = lastName ?? "User"
            };
            
            Console.WriteLine($"   Request data: {System.Text.Json.JsonSerializer.Serialize(requestData)}");
            
            var response = await _httpClient.PostAsJsonAsync("users", requestData);
            
            Console.WriteLine($"   Response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"   Error response: {errorContent}");
                throw new Exception($"HTTP {response.StatusCode}: {errorContent}");
            }

            Console.WriteLine("✅ User creation successful");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creating user: {ex.Message}");
            throw; // Re-throw to let the UI handle the detailed error
        }
    }

    public async Task<bool> EnsureUserExistsAsync(string emailAddress, string firstName, string lastName)
    {
        try
        {
            Console.WriteLine($"🔍 Checking if user exists: {emailAddress}");
            
            // Try to get the user first
            var existingUser = await GetUserProfileAsync(emailAddress);
            
            if (existingUser != null)
            {
                Console.WriteLine("✅ User already exists");
                return true;
            }
            
            Console.WriteLine("👤 User doesn't exist, creating...");
            return await CreateUserAsync(emailAddress, firstName, lastName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error ensuring user exists: {ex.Message}");
            throw;
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
            Console.WriteLine($"🔄 Updating user address for: {emailAddress}");
            
            var requestData = new
            {
                EmailAddress = emailAddress,
                Street1 = street1,
                Street2 = street2,
                City = city,
                State = state,
                ZipCode = zipCode,
                Country = country
            };
            
            Console.WriteLine($"   Request data: {System.Text.Json.JsonSerializer.Serialize(requestData)}");
            
            var response = await _httpClient.PostAsJsonAsync($"users/address/{emailAddress}", requestData);
            
            Console.WriteLine($"   Response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"   Error response: {errorContent}");
                throw new Exception($"HTTP {response.StatusCode}: {errorContent}");
            }

            Console.WriteLine("✅ Address update successful");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating user address: {ex.Message}");
            throw; // Re-throw to let the UI handle the detailed error
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
            Console.WriteLine($"🔄 Updating user preferences for: {emailAddress}");
            
            var requestData = new
            {
                EmailAddress = emailAddress,
                ReceiveEmailNotifications = receiveEmailNotifications,
                ReceiveSmsNotifications = receiveSmsNotifications,
                PreferredLanguage = preferredLanguage,
                TimeZone = timeZone,
                Theme = theme
            };
            
            Console.WriteLine($"   Request data: {System.Text.Json.JsonSerializer.Serialize(requestData)}");
            
            var response = await _httpClient.PostAsJsonAsync($"users/preferences/{emailAddress}", requestData);
            
            Console.WriteLine($"   Response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"   Error response: {errorContent}");
                throw new Exception($"HTTP {response.StatusCode}: {errorContent}");
            }

            Console.WriteLine("✅ Preferences update successful");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating user preferences: {ex.Message}");
            throw; // Re-throw to let the UI handle the detailed error
        }
    }
}