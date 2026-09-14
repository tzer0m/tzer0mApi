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
    /// The event's end time.
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// Whether the event is an all-day event, with no specific start time.
    /// </summary>
    public bool IsAllDay { get; set; }

    /// <summary>
    /// The colour assigned to the event's calendar, e.g. "Black" or "Red".
    /// </summary>
    public string Color { get; set; } = "Black";
}