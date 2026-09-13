namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// Configuration for one calendar to fetch events from, and the colour its events should be marked with on the display.
/// </summary>
public class HomeAssistantCalendarConfig
{
    /// <summary>
    /// The calendar's Home Assistant entity ID, e.g. "calendar.personal".
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// The colour to mark this calendar's events with - one of the Spectra 6 panel's inks usable against a white background: "Black", "Red", "Green", "Blue", or "Yellow".
    /// </summary>
    public string Color { get; set; } = "Black";
}