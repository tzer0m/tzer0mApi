using System.Text.Json.Serialization;

namespace tzer0mApi.Models.Kuma;

/// <summary>
/// A single monitor entry within a status page group, as returned by GET /api/status-page/{slug}.
/// </summary>
public class KumaPublicMonitor
{
    /// <summary>
    /// The monitor's id, used to look up its heartbeats and uptime.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// The monitor's display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}