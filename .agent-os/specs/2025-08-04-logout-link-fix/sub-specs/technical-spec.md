# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-04-logout-link-fix/spec.md

> Created: 2025-08-04
> Version: 1.0.0

## Technical Requirements

- **HTTP Method Alignment**: Fix mismatch between GET request (MenuItem Href) and POST endpoint (/logout)
- **Authentication Scheme Termination**: Ensure proper sign-out from both Cookie and OIDC authentication schemes
- **Session Management**: Properly terminate server-side session and clear authentication cookies
- **Navigation Handling**: Implement JavaScript-based logout flow instead of direct link navigation
- **Redirect Logic**: Ensure proper redirection after logout completion

## Problem Analysis

### Current Issue
The logout implementation has a fundamental HTTP method mismatch:

**Frontend**: `<MudMenuItem Href="auth/logout">` - Generates GET request
**Backend**: `group.MapPost("/logout", ...)` - Only accepts POST requests

This results in a 404 error because no GET endpoint exists for `/auth/logout`.

### Authentication Flow Analysis
The current logout endpoint correctly handles:
1. Cookie authentication sign-out (`CookieAuthenticationDefaults.AuthenticationScheme`)
2. OIDC sign-out (`"MicrosoftOidc"`) to properly terminate Keycloak session
3. Redirect handling via `GetAuthProperties(returnUrl)`

## Approach Options

**Option A: Add GET Logout Endpoint (Not Recommended)**
- Pros: Simple fix, minimal frontend changes
- Cons: Security risk (GET requests can be cached, logged, and triggered accidentally), violates HTTP semantics

**Option B: JavaScript Form Submission (Selected)**
- Pros: Secure POST request, maintains existing backend logic, follows HTTP standards
- Cons: Requires JavaScript implementation, slightly more complex

**Option C: Blazor Server Action**
- Pros: Server-side handling, integrated with Blazor lifecycle
- Cons: More complex implementation, requires significant changes to component structure

**Rationale:** Option B is selected because it maintains security best practices by using POST for logout operations while requiring minimal changes to the existing authentication infrastructure.

## Implementation Details

### Frontend Changes
Replace the direct Href navigation with JavaScript-based form submission:

1. **Remove Href**: Change `<MudMenuItem Href="auth/logout">` to use click handler
2. **Add Form Submission**: Create hidden form that posts to `/auth/logout`
3. **JavaScript Integration**: Use Blazor's IJSRuntime to submit form programmatically

### Backend Changes
No changes required - the existing POST endpoint is correctly implemented.

### Files to Modify
- `src/FxExpert.Blazor/FxExpert.Blazor.Client/Layout/MainLayout.razor` - Update logout menu item
- Potentially add JavaScript helper if needed for form submission

## Security Considerations

- **POST Method**: Ensures logout cannot be triggered accidentally via GET requests
- **CSRF Protection**: Form submission inherits CSRF protection from ASP.NET Core
- **Session Termination**: Properly signs out from both local and OIDC authentication
- **Redirect Safety**: Existing redirect logic prevents open redirect vulnerabilities

## External Dependencies

No new external dependencies required. Uses existing:
- MudBlazor components
- ASP.NET Core authentication
- Blazor JavaScript interop (potentially)