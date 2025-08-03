namespace FxExpert.Blazor.Client.Services;

public interface IUserThemeService
{
    Task<string?> GetUserThemeAsync(string emailAddress);
    Task SetUserThemeAsync(string emailAddress, string theme);
}