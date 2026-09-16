using System.Text.Json.Serialization;

namespace tzer0mApi.Models.Kuma;

/// <summary>
/// A single heartbeat check result, as returned by GET /api/status-page/heartbeat/{slug}.
/// </summary>
public class KumaHeartbeatEntry
{
    /// <summary>
    /// The heartbeat status: 0 = down, 1 = up, 2 = pending, 3 = maintenance.
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }
}