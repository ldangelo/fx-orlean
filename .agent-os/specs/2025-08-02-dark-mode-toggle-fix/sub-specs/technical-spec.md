# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-02-dark-mode-toggle-fix/spec.md

> Created: 2025-08-02
> Version: 1.0.0

## Technical Requirements

- **Theme State Management**: Centralized service to manage theme state across the application
- **Browser Persistence**: Use localStorage to persist theme choice for anonymous users  
- **Authentication Integration**: Connect theme state to UserPreferences.Theme for logged-in users
- **Real-time Updates**: Theme changes should be immediately reflected in all components
- **System Preference Detection**: Detect and use system dark/light mode preference as default
- **Blazor Server Compatibility**: Ensure theme service works with Blazor Server architecture
- **Performance**: Theme switching should be instantaneous without page reload

## Approach Options

**Option A: Component-Level State Management**
- Pros: Simple implementation, minimal code changes
- Cons: Not reusable, potential state synchronization issues, hard to test

**Option B: Centralized Theme Service with IJSRuntime** (Selected)
- Pros: Centralized state management, reusable across components, testable, browser persistence
- Cons: Requires JavaScript interop, slightly more complex

**Option C: SignalR-based Theme Broadcasting**
- Pros: Real-time synchronization across multiple browser tabs
- Cons: Overkill for single-user theme preference, adds unnecessary complexity

**Rationale:** Option B provides the best balance of functionality, maintainability, and performance. It centralizes theme management while leveraging browser storage capabilities through JavaScript interop.

## External Dependencies

- **Microsoft.JSInterop** - Built into Blazor (no additional package needed)
- **Justification:** Required for localStorage access from Blazor Server components

## Implementation Architecture

### Core Components

1. **IThemeService Interface**
   - GetCurrentThemeAsync() -> ThemeMode enum
   - SetThemeAsync(ThemeMode theme) -> Task
   - ThemeChanged event for notifications

2. **ThemeService Implementation**
   - Manages theme state using IJSRuntime
   - Handles localStorage persistence
   - Integrates with UserService for authenticated users

3. **ThemeMode Enum**
   - Light, Dark, System (follows browser preference)

4. **JavaScript Helper**
   - localStorage read/write operations
   - System preference detection
   - Theme change notifications

### Data Flow

1. **Initial Load**: Service checks user preference -> localStorage -> system preference
2. **Theme Change**: User clicks toggle -> Service updates state -> localStorage/API -> Components refresh
3. **Authentication**: Login triggers theme sync from UserPreferences -> localStorage

### Integration Points

- **MainLayout.razor**: Subscribe to theme changes, apply to MudThemeProvider
- **UserService.cs**: Add theme persistence methods
- **Program.cs**: Register ThemeService as scoped service