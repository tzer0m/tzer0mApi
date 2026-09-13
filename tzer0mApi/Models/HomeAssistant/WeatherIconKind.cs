namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// The small set of icon shapes drawn for a Home Assistant weather condition.
/// </summary>
public enum WeatherIconKind
{
    /// <summary>
    /// Clear sky, daytime.
    /// </summary>
    Sunny,

    /// <summary>
    /// Clear sky, night time.
    /// </summary>
    ClearNight,

    /// <summary>
    /// Sun with some cloud.
    /// </summary>
    PartlyCloudy,

    /// <summary>
    /// Overcast or foggy.
    /// </summary>
    Cloudy,

    /// <summary>
    /// Rain or drizzle.
    /// </summary>
    Rain,

    /// <summary>
    /// Snow or sleet.
    /// </summary>
    Snow,

    /// <summary>
    /// Thunderstorms.
    /// </summary>
    Thunder,

    /// <summary>
    /// High wind.
    /// </summary>
    Windy
}