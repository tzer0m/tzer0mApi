namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// The current weather conditions pulled from a Home Assistant weather entity.
/// </summary>
public class HomeAssistantWeather
{
    /// <summary>
    /// A short, human-readable label for the current condition, e.g. "Partly Cloudy".
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The icon shape to draw for the current condition.
    /// </summary>
    public WeatherIconKind IconKind { get; set; }

    /// <summary>
    /// The current temperature, in degrees Celsius.
    /// </summary>
    public double TemperatureC { get; set; }
}