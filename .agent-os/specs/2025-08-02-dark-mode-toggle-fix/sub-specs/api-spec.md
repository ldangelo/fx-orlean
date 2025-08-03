# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-08-02-dark-mode-toggle-fix/spec.md

> Created: 2025-08-02
> Version: 1.0.0

## Endpoints

### GET /api/users/{emailAddress}/preferences/theme

**Purpose:** Retrieve the current theme preference for an authenticated user
**Parameters:** 
- emailAddress (string): User's email address from authentication context
**Response:** 
```json
{
  "theme": "Light" | "Dark" | "System"
}
```
**Errors:** 
- 404 Not Found: User not found
- 401 Unauthorized: User not authenticated

### PUT /api/users/{emailAddress}/preferences/theme

**Purpose:** Update the theme preference for an authenticated user
**Parameters:**
- emailAddress (string): User's email address from authentication context
**Request Body:**
```json
{
  "theme": "Light" | "Dark" | "System"
}
```
**Response:** 
```json
{
  "success": true,
  "theme": "Light" | "Dark" | "System"
}
```
**Errors:**
- 400 Bad Request: Invalid theme value
- 404 Not Found: User not found  
- 401 Unauthorized: User not authenticated

## Controllers

### ThemeController Actions

**GetUserTheme**
- Business Logic: Retrieve user preferences and extract theme setting
- Error Handling: Return appropriate HTTP status codes for authentication/authorization failures

**UpdateUserTheme** 
- Business Logic: Validate theme value, update user preferences, persist to database
- Error Handling: Validate input, handle database errors gracefully

## Integration with Existing UserController

The theme endpoints can be integrated into the existing UserController as additional actions, following the established pattern of user preference management. This maintains consistency with the current API structure.

### Method Signatures

```csharp
[HttpGet("api/users/{emailAddress}/preferences/theme")]
public async Task<IActionResult> GetUserTheme(string emailAddress)

[HttpPut("api/users/{emailAddress}/preferences/theme")]  
public async Task<IActionResult> UpdateUserTheme(string emailAddress, [FromBody] ThemeUpdateRequest request)
```