using System.Text.Json.Serialization;

namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// The raw start/end shape of a Home Assistant calendar event - either a specific time or an all-day date.
/// </summary>
public class HomeAssistantCalendarDateTimeResponse
{
    /// <summary>
    /// The specific date and time, set for timed events.
    /// </summary>
    [JsonPropertyName("dateTime")]
    public DateTime? DateTimeValue { get; set; }

    /// <summary>
    /// The date only, set for all-day events.
    /// </summary>
    [JsonPropertyName("date")]
    public DateOnly? Date { get; set; }
}