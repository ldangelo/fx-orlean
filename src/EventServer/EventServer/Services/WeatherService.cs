using System;
using EventServer.Models;

namespace EventServer.Services;


public interface IWeatherService {
    public WeatherForecast[] GetWeather();
}

public class WeatherService : IWeatherService
{
    private WeatherForecast[]? forecasts;

    public WeatherForecast[] GetWeather()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        if (forecasts == null)
        {
            var rng = new Random();
            forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = summaries[rng.Next(summaries.Length)]
            }).ToArray();
        }
        return forecasts;
    }
}
