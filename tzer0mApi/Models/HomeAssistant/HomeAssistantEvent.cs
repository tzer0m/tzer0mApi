namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// A single calendar event pulled from Home Assistant for today's event list.
/// </summary>
public class HomeAssistantEvent
{
    /// <summary>
    /// The event's title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The event's start time.
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Whether the event is an all-day event, with no specific start time.
    /// </summary>
    public bool IsAllDay { get; set; }
}