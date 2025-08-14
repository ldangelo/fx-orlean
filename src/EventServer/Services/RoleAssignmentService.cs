using System.Text.RegularExpressions;

namespace EventServer.Services;

/// <summary>
/// Service for determining user roles based on email domain
/// </summary>
public static class RoleAssignmentService
{
    private const string FORTIUM_DOMAIN = "@fortiumpartners.com";
    private const string PARTNER_ROLE = "PARTNER";
    private const string CLIENT_ROLE = "CLIENT";
    
    // Email validation regex pattern
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// Determines the appropriate user role based on email domain
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>PARTNER for @fortiumpartners.com emails, CLIENT for all others</returns>
    public static string DetermineUserRole(string? email)
    {
        // Handle null, empty, or whitespace emails
        if (string.IsNullOrWhiteSpace(email))
        {
            return CLIENT_ROLE;
        }

        // Validate email format
        if (!IsValidEmailFormat(email))
        {
            return CLIENT_ROLE;
        }

        // Check if email ends with Fortium domain (case-insensitive)
        return email.EndsWith(FORTIUM_DOMAIN, StringComparison.OrdinalIgnoreCase) 
            ? PARTNER_ROLE 
            : CLIENT_ROLE;
    }

    /// <summary>
    /// Validates basic email format
    /// </summary>
    /// <param name="email">Email to validate</param>
    /// <returns>True if email format is valid</returns>
    private static bool IsValidEmailFormat(string email)
    {
        try
        {
            // Basic validation using regex
            return EmailRegex.IsMatch(email) && 
                   email.Count(c => c == '@') == 1; // Ensure exactly one @ symbol
        }
        catch
        {
            // If regex fails for any reason, default to invalid
            return false;
        }
    }

    /// <summary>
    /// Gets the Fortium domain constant for testing/validation purposes
    /// </summary>
    public static string FortiumDomain => FORTIUM_DOMAIN;

    /// <summary>
    /// Gets the partner role constant
    /// </summary>
    public static string PartnerRole => PARTNER_ROLE;

    /// <summary>
    /// Gets the client role constant
    /// </summary>
    public static string ClientRole => CLIENT_ROLE;
}