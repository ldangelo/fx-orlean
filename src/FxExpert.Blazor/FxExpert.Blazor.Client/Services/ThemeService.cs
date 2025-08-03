using Microsoft.JSInterop;

namespace FxExpert.Blazor.Client.Services;

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IUserThemeService _userThemeService;
    private readonly ILogger<ThemeService> _logger;
    private const string ThemeStorageKey = "theme";
    private string? _currentUserEmail;
    private IJSObjectReference? _systemThemeWatcher;
    private bool _isMonitoringSystemTheme;

    public event EventHandler<ThemeMode>? ThemeChanged;
    public event EventHandler<string>? SystemThemeChanged;

    public ThemeService(IJSRuntime jsRuntime, IUserThemeService userThemeService, ILogger<ThemeService> logger)
    {
        _jsRuntime = jsRuntime;
        _userThemeService = userThemeService;
        _logger = logger;
    }

    public async Task<ThemeMode> GetCurrentThemeAsync()
    {
        try
        {
            var storedTheme = await _jsRuntime.InvokeAsync<string>("ThemeHelpers.getStoredTheme");
            
            if (string.IsNullOrEmpty(storedTheme))
            {
                return ThemeMode.Light;
            }

            if (Enum.TryParse<ThemeMode>(storedTheme, out var theme))
            {
                return theme;
            }

            return ThemeMode.Light;
        }
        catch
        {
            // If JavaScript interop fails, return default theme
            return ThemeMode.Light;
        }
    }

    public async Task SetThemeAsync(ThemeMode theme)
    {
        try
        {
            // Save to local storage
            await _jsRuntime.InvokeAsync<bool>("ThemeHelpers.setStoredTheme", theme.ToString());
            
            // Save to user account if authenticated
            if (!string.IsNullOrEmpty(_currentUserEmail))
            {
                await SaveUserThemeAsync(_currentUserEmail, theme);
            }
            
            // Apply the theme
            await ApplyThemeAsync(theme);
            
            // Notify listeners
            ThemeChanged?.Invoke(this, theme);
        }
        catch
        {
            // Handle JavaScript interop failures gracefully
            // Theme change will still be notified to UI even if storage fails
            ThemeChanged?.Invoke(this, theme);
        }
    }

    public async Task<string> GetSystemThemeAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("ThemeHelpers.getSystemTheme");
        }
        catch
        {
            // If JavaScript interop fails, return light as default
            return "Light";
        }
    }

    public async Task<string> GetEffectiveThemeAsync(ThemeMode theme)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("ThemeHelpers.getEffectiveTheme", theme.ToString());
        }
        catch
        {
            // If JavaScript interop fails, resolve manually
            if (theme == ThemeMode.System)
            {
                return await GetSystemThemeAsync();
            }
            return theme.ToString();
        }
    }

    public async Task ApplyThemeAsync(ThemeMode theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("ThemeHelpers.applyTheme", theme.ToString(), null);
        }
        catch
        {
            // If JavaScript interop fails, theme application will be handled by UI components
        }
    }

    public async Task LoadUserThemeAsync(string? userEmail)
    {
        _currentUserEmail = userEmail;
        
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogDebug("No user email provided, using local theme storage only");
            return;
        }

        try
        {
            _logger.LogInformation("Loading theme for user {UserEmail}", userEmail);
            var userTheme = await _userThemeService.GetUserThemeAsync(userEmail);
            
            if (!string.IsNullOrEmpty(userTheme) && Enum.TryParse<ThemeMode>(userTheme, out var themeMode))
            {
                _logger.LogInformation("Found user theme {Theme} for {UserEmail}", userTheme, userEmail);
                
                // Update local storage to match user preference
                await _jsRuntime.InvokeAsync<bool>("ThemeHelpers.setStoredTheme", userTheme);
                
                // Apply the theme
                await ApplyThemeAsync(themeMode);
                
                // Notify listeners
                ThemeChanged?.Invoke(this, themeMode);
            }
            else
            {
                _logger.LogInformation("No theme found for user {UserEmail}, keeping current theme", userEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading theme for user {UserEmail}", userEmail);
        }
    }

    public async Task SaveUserThemeAsync(string? userEmail, ThemeMode theme)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogDebug("No user email provided, saving to local storage only");
            return;
        }

        try
        {
            _logger.LogInformation("Saving theme {Theme} for user {UserEmail}", theme, userEmail);
            await _userThemeService.SetUserThemeAsync(userEmail, theme.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving theme {Theme} for user {UserEmail}", theme, userEmail);
        }
    }

    public async Task StartSystemThemeMonitoringAsync()
    {
        if (_isMonitoringSystemTheme)
        {
            _logger.LogDebug("System theme monitoring is already active");
            return;
        }

        try
        {
            _logger.LogInformation("Starting system theme monitoring");
            
            // Create a callback function for system theme changes
            var dotNetObjectRef = DotNetObjectReference.Create(this);
            
            // Start watching system theme changes
            _systemThemeWatcher = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "ThemeHelpers.watchSystemTheme", 
                dotNetObjectRef);
                
            _isMonitoringSystemTheme = true;
            _logger.LogInformation("System theme monitoring started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start system theme monitoring");
        }
    }

    public async Task StopSystemThemeMonitoringAsync()
    {
        if (!_isMonitoringSystemTheme || _systemThemeWatcher == null)
        {
            _logger.LogDebug("System theme monitoring is not active");
            return;
        }

        try
        {
            _logger.LogInformation("Stopping system theme monitoring");
            
            // Stop watching system theme changes
            await _systemThemeWatcher.InvokeVoidAsync("stop");
            await _systemThemeWatcher.DisposeAsync();
            
            _systemThemeWatcher = null;
            _isMonitoringSystemTheme = false;
            
            _logger.LogInformation("System theme monitoring stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop system theme monitoring");
        }
    }

    [JSInvokable]
    public async Task OnSystemThemeChanged(bool isDark)
    {
        var newSystemTheme = isDark ? "Dark" : "Light";
        _logger.LogInformation("System theme changed to {SystemTheme}", newSystemTheme);
        
        // Notify listeners of system theme change
        SystemThemeChanged?.Invoke(this, newSystemTheme);
        
        // If current theme is set to System, update the effective theme
        var currentTheme = await GetCurrentThemeAsync();
        if (currentTheme == ThemeMode.System)
        {
            _logger.LogInformation("Current theme is System mode, applying new system theme {SystemTheme}", newSystemTheme);
            
            // Apply the new system theme
            await ApplyThemeAsync(ThemeMode.System);
            
            // Notify that the effective theme has changed
            ThemeChanged?.Invoke(this, ThemeMode.System);
        }
    }
}