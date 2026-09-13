using System.Text.Json.Serialization;

namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// The raw shape of a single to-do item, as returned by the todo.get_items service call.
/// </summary>
public class HomeAssistantTodoItemResponse
{
    /// <summary>
    /// The item's title.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// The item's completion status, e.g. "needs_action" or "completed".
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// The item's due date, as a date-only string.
    /// </summary>
    [JsonPropertyName("due")]
    public string? Due { get; set; }
}