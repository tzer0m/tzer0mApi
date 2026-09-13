using System.Text.Json;
using System.Text.Json.Serialization;

namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// The raw shape of a Home Assistant entity state, as returned by GET /api/states/{entity_id}.
/// </summary>
public class HomeAssistantStateResponse
{
    /// <summary>
    /// The entity's current state, e.g. a weather condition slug like "partlycloudy".
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// The entity's attributes, e.g. temperature for a weather entity.
    /// </summary>
    [JsonPropertyName("attributes")]
    public Dictionary<string, JsonElement> Attributes { get; set; } = [];
}