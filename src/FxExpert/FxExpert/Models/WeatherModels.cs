namespace FxExpert.Models;

public class GetWeatherForecastRequest
{
    // Empty request since this is a GET endpoint
}

public class WeatherForecast 
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
