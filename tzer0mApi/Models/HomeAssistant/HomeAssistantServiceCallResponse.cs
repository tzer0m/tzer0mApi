using System.Text.Json.Serialization;

namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// The raw envelope Home Assistant wraps a service call's response data in.
/// </summary>
public class HomeAssistantServiceCallResponse
{
    /// <summary>
    /// The response data, keyed by the entity ID the service call targeted.
    /// </summary>
    [JsonPropertyName("service_response")]
    public Dictionary<string, HomeAssistantTodoListResponse> ServiceResponse { get; set; } = [];
}