using Microsoft.AspNetCore.Authorization;

namespace FxExpert.Blazor.Components.Account;

/// <summary>
/// Custom attribute to require specific roles for access to components or pages
/// </summary>
public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params string[] roles)
    {
        if (roles.Length > 0)
        {
            Roles = string.Join(",", roles);
        }
    }
}