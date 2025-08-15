using System.Security.Claims;
using System.Text.Json;

namespace FxExpert.Blazor.Services;

/// <summary>
/// Service for integrating Keycloak authentication with EventServer user management
/// Ensures users are created/updated in EventServer during authentication
/// </summary>
public interface IAuthenticationIntegrationService
{
    /// <summary>
    /// Ensures user exists in EventServer and assigns appropriate role based on email domain
    /// </summary>
    /// <param name="principal">Claims principal from successful authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user was successfully processed</returns>
    Task<bool> EnsureUserExistsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

public class AuthenticationIntegrationService : IAuthenticationIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationIntegrationService> _logger;

    public AuthenticationIntegrationService(
        IHttpClientFactory httpClientFactory, 
        ILogger<AuthenticationIntegrationService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("EventServer");
        _logger = logger;
    }

    public async Task<bool> EnsureUserExistsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract user information from claims
            var email = GetEmailFromClaims(principal);
            var firstName = GetFirstNameFromClaims(principal);
            var lastName = GetLastNameFromClaims(principal);

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("No email claim found in authentication principal");
                return false;
            }

            _logger.LogInformation("Ensuring user exists in EventServer: {Email}", email);

            // Check if user already exists
            var existingUser = await GetUserAsync(email, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogInformation("User already exists in EventServer: {Email}", email);
                return true;
            }

            // Create user with automatic role assignment based on email domain
            var createUserRequest = new CreateUserRequest
            {
                FirstName = firstName ?? "Unknown",
                LastName = lastName ?? "User", 
                EmailAddress = email,
                // Role will be automatically assigned by RoleAssignmentService
                Role = null
            };

            var success = await CreateUserAsync(createUserRequest, cancellationToken);
            if (success)
            {
                _logger.LogInformation("Successfully created user in EventServer: {Email}", email);
            }
            else
            {
                _logger.LogError("Failed to create user in EventServer: {Email}", email);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring user exists in EventServer");
            return false;
        }
    }

    private string? GetEmailFromClaims(ClaimsPrincipal principal)
    {
        // Try multiple email claim types that might be used
        var emailClaim = principal.FindFirst(ClaimTypes.Email) 
                        ?? principal.FindFirst("email")
                        ?? principal.FindFirst("preferred_username")
                        ?? principal.FindFirst("upn");
        
        return emailClaim?.Value;
    }

    private string? GetFirstNameFromClaims(ClaimsPrincipal principal)
    {
        var firstNameClaim = principal.FindFirst(ClaimTypes.GivenName)
                           ?? principal.FindFirst("given_name")
                           ?? principal.FindFirst("first_name");
        
        return firstNameClaim?.Value;
    }

    private string? GetLastNameFromClaims(ClaimsPrincipal principal)
    {
        var lastNameClaim = principal.FindFirst(ClaimTypes.Surname)
                          ?? principal.FindFirst("family_name")
                          ?? principal.FindFirst("last_name");
        
        return lastNameClaim?.Value;
    }

    private async Task<UserResponse?> GetUserAsync(string email, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/users/{Uri.EscapeDataString(email)}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<UserResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists: {Email}", email);
            return null;
        }
    }

    private async Task<bool> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/users", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("User created successfully: {Email}", request.EmailAddress);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create user. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", request.EmailAddress);
            return false;
        }
    }
}

// DTOs for EventServer communication
public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string? Role { get; set; }
}

public class UserResponse
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool Active { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
}