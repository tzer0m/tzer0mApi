using System.Text.Json.Serialization;

namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// The raw shape of one to-do list's items, as returned within a todo.get_items service response.
/// </summary>
public class HomeAssistantTodoListResponse
{
    /// <summary>
    /// The list's items.
    /// </summary>
    [JsonPropertyName("items")]
    public List<HomeAssistantTodoItemResponse> Items { get; set; } = [];
}