using System.Text.Json.Serialization;

namespace tzer0mApi.Models.Kuma;

/// <summary>
/// The raw shape of a status page's heartbeat data, as returned by GET /api/status-page/heartbeat/{slug}.
/// </summary>
public class KumaHeartbeatPageResponse
{
    /// <summary>
    /// Each monitor's recent heartbeats, keyed by monitor id, oldest first - Kuma only retains the most recent ~100 checks here, not a fixed time window.
    /// </summary>
    [JsonPropertyName("heartbeatList")]
    public Dictionary<string, List<KumaHeartbeatEntry>> HeartbeatList { get; set; } = [];

    /// <summary>
    /// Each monitor's rolling uptime ratio, keyed as "{monitorId}_{periodInHours}" (e.g. "6_24" is monitor 6's 24-hour uptime), each a 0-1 ratio.
    /// </summary>
    [JsonPropertyName("uptimeList")]
    public Dictionary<string, double> UptimeList { get; set; } = [];
}