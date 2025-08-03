# Spec Tasks

These are the tasks to be completed for the spec detailed in @.agent-os/specs/2025-08-02-dark-mode-toggle-fix/spec.md

> Created: 2025-08-02
> Status: Ready for Implementation

## Tasks

- [ ] 1. Create Theme Service Infrastructure
  - [ ] 1.1 Write tests for IThemeService interface and ThemeMode enum
  - [ ] 1.2 Create IThemeService interface with required methods and events
  - [ ] 1.3 Create ThemeMode enum (Light, Dark, System)
  - [ ] 1.4 Implement ThemeService class with IJSRuntime integration
  - [ ] 1.5 Add theme service registration to Program.cs
  - [ ] 1.6 Verify all theme service tests pass

- [ ] 2. Implement JavaScript Theme Helpers
  - [ ] 2.1 Write tests for JavaScript localStorage operations
  - [ ] 2.2 Create theme.js file with localStorage helper functions
  - [ ] 2.3 Add system preference detection (prefers-color-scheme)
  - [ ] 2.4 Include theme.js in wwwroot and reference in _Host.cshtml
  - [ ] 2.5 Verify JavaScript functions work with test page
  - [ ] 2.6 Verify all JavaScript integration tests pass

- [ ] 3. Update MainLayout Theme Integration
  - [ ] 3.1 Write tests for MainLayout theme state management
  - [ ] 3.2 Inject IThemeService into MainLayout component
  - [ ] 3.3 Replace local _isDarkMode with service-managed state
  - [ ] 3.4 Subscribe to ThemeChanged events for real-time updates
  - [ ] 3.5 Update DarkModeToggle method to use theme service
  - [ ] 3.6 Verify MainLayout theme tests pass and UI updates correctly

- [ ] 4. Extend API for Theme Persistence
  - [ ] 4.1 Write tests for new theme API endpoints
  - [ ] 4.2 Add GetUserTheme action to UserController
  - [ ] 4.3 Add UpdateUserTheme action to UserController
  - [ ] 4.4 Update UserService with GetUserThemeAsync and UpdateUserThemeAsync methods
  - [ ] 4.5 Add theme-specific request/response DTOs
  - [ ] 4.6 Verify all API tests pass and endpoints work correctly

- [ ] 5. Implement Authentication Integration
  - [ ] 5.1 Write tests for authenticated user theme synchronization
  - [ ] 5.2 Add user authentication state monitoring to ThemeService
  - [ ] 5.3 Implement theme sync on login (load from UserPreferences)
  - [ ] 5.4 Implement theme persistence on logout (save to localStorage only)
  - [ ] 5.5 Add error handling for theme API failures
  - [ ] 5.6 Verify authentication integration tests pass

- [ ] 6. Add System Theme Detection
  - [ ] 6.1 Write tests for system preference detection and handling
  - [ ] 6.2 Implement JavaScript system theme detection
  - [ ] 6.3 Add System option to theme toggle UI (optional enhancement)
  - [ ] 6.4 Handle system theme changes at runtime
  - [ ] 6.5 Set appropriate defaults for new users
  - [ ] 6.6 Verify system theme detection tests pass

- [ ] 7. Integration Testing and Bug Fixes
  - [ ] 7.1 Write comprehensive end-to-end tests for complete theme workflows
  - [ ] 7.2 Test anonymous user theme persistence across browser refresh
  - [ ] 7.3 Test authenticated user theme synchronization across devices/browsers
  - [ ] 7.4 Test login/logout theme transition scenarios
  - [ ] 7.5 Fix any discovered issues and edge cases
  - [ ] 7.6 Verify all integration tests pass and workflows work as expected