using System.Text.Json.Serialization;

namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// The raw shape of a single event, as returned by GET /api/calendars/{entity_id}.
/// </summary>
public class HomeAssistantCalendarEventResponse
{
    /// <summary>
    /// The event's title.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// The event's start.
    /// </summary>
    [JsonPropertyName("start")]
    public HomeAssistantCalendarDateTimeResponse Start { get; set; } = new();
}