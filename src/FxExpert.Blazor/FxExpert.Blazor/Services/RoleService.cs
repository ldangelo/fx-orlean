using System.Security.Claims;

namespace FxExpert.Blazor.Services;

/// <summary>
/// Service for working with user roles from authentication claims
/// Provides easy access to EventServer-assigned roles
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Determines if the current user has the specified role
    /// </summary>
    /// <param name="role">Role to check (PARTNER, CLIENT, etc.)</param>
    /// <param name="user">Claims principal to check (uses current user if null)</param>
    /// <returns>True if user has the specified role</returns>
    bool HasRole(string role, ClaimsPrincipal? user = null);

    /// <summary>
    /// Gets the EventServer-assigned role for the current user
    /// </summary>
    /// <param name="user">Claims principal to check (uses current user if null)</param>
    /// <returns>The assigned role (PARTNER, CLIENT) or null if not found</returns>
    string? GetAssignedRole(ClaimsPrincipal? user = null);

    /// <summary>
    /// Determines if the current user is a Fortium partner
    /// </summary>
    /// <param name="user">Claims principal to check (uses current user if null)</param>
    /// <returns>True if user has PARTNER role</returns>
    bool IsPartner(ClaimsPrincipal? user = null);

    /// <summary>
    /// Determines if the current user is a client
    /// </summary>
    /// <param name="user">Claims principal to check (uses current user if null)</param>
    /// <returns>True if user has CLIENT role</returns>
    bool IsClient(ClaimsPrincipal? user = null);

    /// <summary>
    /// Gets all roles assigned to the current user
    /// </summary>
    /// <param name="user">Claims principal to check (uses current user if null)</param>
    /// <returns>List of all role claims</returns>
    IEnumerable<string> GetAllRoles(ClaimsPrincipal? user = null);
}

public class RoleService : IRoleService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IHttpContextAccessor httpContextAccessor, ILogger<RoleService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public bool HasRole(string role, ClaimsPrincipal? user = null)
    {
        var principal = user ?? GetCurrentUser();
        if (principal?.Identity?.IsAuthenticated != true)
            return false;

        // Check multiple role claim types for flexibility
        var hasRole = principal.IsInRole(role) ||
                     principal.HasClaim("fx_role", role) ||
                     principal.HasClaim("application_role", role) ||
                     principal.HasClaim(ClaimTypes.Role, role);

        _logger.LogDebug("Role check for {Role}: {HasRole}", role, hasRole);
        return hasRole;
    }

    public string? GetAssignedRole(ClaimsPrincipal? user = null)
    {
        var principal = user ?? GetCurrentUser();
        if (principal?.Identity?.IsAuthenticated != true)
            return null;

        // Check EventServer-specific claims first, then fall back to standard role claims
        var role = principal.FindFirst("fx_role")?.Value ??
                  principal.FindFirst("application_role")?.Value ??
                  principal.FindFirst(ClaimTypes.Role)?.Value;

        _logger.LogDebug("Retrieved assigned role: {Role}", role);
        return role;
    }

    public bool IsPartner(ClaimsPrincipal? user = null)
    {
        return HasRole("PARTNER", user);
    }

    public bool IsClient(ClaimsPrincipal? user = null)
    {
        return HasRole("CLIENT", user);
    }

    public IEnumerable<string> GetAllRoles(ClaimsPrincipal? user = null)
    {
        var principal = user ?? GetCurrentUser();
        if (principal?.Identity?.IsAuthenticated != true)
            return Enumerable.Empty<string>();

        var roles = principal.Claims
            .Where(c => c.Type == ClaimTypes.Role || 
                       c.Type == "fx_role" || 
                       c.Type == "application_role" ||
                       c.Type == "role")
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        _logger.LogDebug("Retrieved all roles: {Roles}", string.Join(", ", roles));
        return roles;
    }

    private ClaimsPrincipal? GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User;
    }
}