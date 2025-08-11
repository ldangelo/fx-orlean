using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});
builder.Services.AddTransient<AntiforgeryHandler>();

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure logging
builder.Services.AddLogging();

// Register services
builder.Services.AddScoped<FxExpert.Blazor.Client.Services.UserService>();
builder.Services.AddScoped<FxExpert.Blazor.Client.Services.IThemeService, FxExpert.Blazor.Client.Services.ThemeService>();
builder.Services.AddScoped<FxExpert.Blazor.Client.Services.IUserThemeService, FxExpert.Blazor.Client.Services.UserThemeService>();

// Register filter services (both original and optimized)
builder.Services.AddScoped<FxExpert.Blazor.Client.Services.FilterService>();
builder.Services.AddScoped<FxExpert.Blazor.Client.Services.ICalendarHttpService, FxExpert.Blazor.Client.Services.CalendarHttpService>();
builder.Services.AddScoped<FxExpert.Blazor.Client.Services.IOptimizedFilterService, FxExpert.Blazor.Client.Services.OptimizedFilterService>();

// Register performance monitoring service
builder.Services.AddSingleton<FxExpert.Blazor.Client.Services.IPerformanceMonitoringService, FxExpert.Blazor.Client.Services.PerformanceMonitoringService>();


// Add authentication and authorization
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

var app = builder.Build();

public class AntiforgeryHandler : DelegatingHandler
{
  protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
      CancellationToken cancellationToken)
  {
    request.Headers.Add("X-CSRF", "1");
    return base.SendAsync(request, cancellationToken);
  }
}
