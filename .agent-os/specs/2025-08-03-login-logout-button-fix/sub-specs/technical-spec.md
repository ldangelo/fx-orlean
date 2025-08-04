# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-03-login-logout-button-fix/spec.md

> Created: 2025-08-03
> Version: 1.0.0

## Technical Requirements

- **Authentication State Detection**: Fix the role-based `AuthorizeView` logic to properly detect authenticated users regardless of specific role assignments
- **Claims Mapping**: Ensure Keycloak JWT tokens are properly decoded and user claims are accessible in Blazor components
- **Authorization Logic**: Update the `AuthorizeView` condition to use authentication state rather than specific role requirements
- **User Identity Display**: Maintain proper user name display from JWT claims in the user menu
- **Debug Consistency**: Ensure debug information accurately reflects the same authentication state used by the UI

## Approach Options

**Option A: Role-Based Authorization (Current)**
- Pros: Provides granular access control for different user types
- Cons: Requires proper role mapping from Keycloak, complex configuration, currently not working

**Option B: Authentication-Based Authorization (Selected)**
- Pros: Simple authentication check, works with any authenticated user, easier to maintain
- Cons: Less granular control, requires separate role checks for specific features

**Option C: Mixed Approach**
- Pros: Authentication for basic UI, roles for specific features
- Cons: More complex logic, potential for inconsistency

**Rationale:** Option B is selected because the primary issue is that authenticated users can't access basic navigation. Once basic authentication detection works, role-based features can be added incrementally. The current role mapping appears to be the root cause of the UI issue.

## Implementation Details

### Current Issue Analysis

The MainLayout.razor uses `<AuthorizeView Roles="User">` but the debug output shows `User: False` even for authenticated users. This suggests:

1. **Role Mapping Issue**: Keycloak JWT tokens may not include the expected "User" role claim
2. **Claims Transformation**: The role claims from Keycloak might not be properly transformed into .NET role claims
3. **Role Configuration**: The Keycloak client or realm configuration may not be assigning the expected roles

### Proposed Solution

1. **Change Authorization Logic**: Replace `<AuthorizeView Roles="User">` with `<AuthorizeView>`
2. **Verify Claims Processing**: Ensure JWT token claims are properly mapped to ClaimsPrincipal
3. **Update Debug Display**: Show both authentication state and available claims for troubleshooting
4. **Test Authentication Flow**: Verify the change works with the existing Keycloak integration

### Files to Modify

- `src/FxExpert.Blazor/FxExpert.Blazor.Client/Layout/MainLayout.razor` - Update AuthorizeView logic
- Potentially authentication configuration files if claims mapping needs adjustment

## External Dependencies

No new external dependencies required. This fix uses existing authentication infrastructure and MudBlazor components.