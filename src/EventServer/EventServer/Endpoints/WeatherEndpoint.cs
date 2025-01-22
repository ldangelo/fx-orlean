using EventServer.Models;
using EventServer.Services;
using FastEndpoints;

namespace EventServer.Endpoints;

public class WeatherEndpoint : EndpointWithoutRequest<IEnumerable<WeatherForecast>>
{
    private readonly IWeatherService _weatherService;

    public WeatherEndpoint(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public override void Configure()
    {
        Get("/api/weather");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var forecasts = _weatherService.GetWeather();
        await SendAsync(forecasts, cancellation: ct);
    }
}
