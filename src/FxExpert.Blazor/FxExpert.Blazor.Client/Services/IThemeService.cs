namespace FxExpert.Blazor.Client.Services;

public enum ThemeMode
{
    Light,
    Dark,
    System
}

public interface IThemeService
{
    event EventHandler<ThemeMode>? ThemeChanged;
    event EventHandler<string>? SystemThemeChanged;
    Task<ThemeMode> GetCurrentThemeAsync();
    Task SetThemeAsync(ThemeMode theme);
    Task<string> GetSystemThemeAsync();
    Task<string> GetEffectiveThemeAsync(ThemeMode theme);
    Task ApplyThemeAsync(ThemeMode theme);
    Task LoadUserThemeAsync(string? userEmail);
    Task SaveUserThemeAsync(string? userEmail, ThemeMode theme);
    Task StartSystemThemeMonitoringAsync();
    Task StopSystemThemeMonitoringAsync();
}