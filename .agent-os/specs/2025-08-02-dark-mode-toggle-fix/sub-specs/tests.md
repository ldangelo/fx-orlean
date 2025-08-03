# Tests Specification

This is the tests coverage details for the spec detailed in @.agent-os/specs/2025-08-02-dark-mode-toggle-fix/spec.md

> Created: 2025-08-02
> Version: 1.0.0

## Test Coverage

### Unit Tests

**ThemeService**
- GetCurrentThemeAsync returns correct default theme (Light)
- GetCurrentThemeAsync returns theme from localStorage when available
- SetThemeAsync updates localStorage and fires ThemeChanged event
- SetThemeAsync calls UserService for authenticated users
- System theme detection works correctly
- Theme persistence survives service recreation

**ThemeController**
- GetUserTheme returns user's theme preference
- GetUserTheme returns 404 for non-existent user
- UpdateUserTheme successfully updates user preferences
- UpdateUserTheme validates theme enum values
- UpdateUserTheme returns 400 for invalid theme values

**UserService Theme Methods**
- GetUserThemeAsync makes correct API call
- UpdateUserThemeAsync sends proper request body
- Error handling for network failures
- Authentication token handling

### Integration Tests

**Theme API Endpoints**
- GET /api/users/{email}/preferences/theme returns correct theme
- PUT /api/users/{email}/preferences/theme updates database
- Authentication required for theme endpoints
- Theme values persist through user preference updates

**Theme Service Integration**
- Theme changes trigger UI updates in MainLayout
- localStorage persistence across browser refresh
- Theme synchronization after user login
- Multiple components receive theme change notifications

**End-to-End Workflow**
- Anonymous user: set theme -> persists in localStorage -> survives refresh
- Authenticated user: set theme -> saves to database -> syncs across devices
- Login workflow: existing localStorage theme syncs to user preferences
- Logout workflow: reverts to localStorage or system preference

### Mocking Requirements

- **IJSRuntime:** Mock localStorage interactions and system preference detection
- **HttpClient:** Mock API responses for theme preference endpoints  
- **AuthenticationStateProvider:** Mock authentication state for user context
- **UserService:** Mock for testing theme service without network calls

## Test Data Setup

### User Test Data
```csharp
var testUser = new User 
{ 
  EmailAddress = "test@example.com",
  Preferences = new UserPreferences { Theme = "Dark" }
};
```

### JavaScript Interop Mocks
```csharp
mockJSRuntime.Setup(x => x.InvokeAsync<string>("localStorage.getItem", "theme"))
           .ReturnsAsync("Dark");

mockJSRuntime.Setup(x => x.InvokeAsync<bool>("matchMedia", "(prefers-color-scheme: dark)"))
           .ReturnsAsync(true);
```

## Performance Tests

- Theme switching should complete within 100ms
- localStorage operations should not block UI thread
- Theme change notifications should not cause memory leaks
- Multiple rapid theme toggles should be handled gracefully