using System.Text.Json.Serialization;

namespace tzer0mApi.Models.Kuma;

/// <summary>
/// A status page group - one per host on Tom's Kuma instance - as returned by GET /api/status-page/{slug}.
/// </summary>
public class KumaPublicGroup
{
    /// <summary>
    /// The group's name, which is the host name (e.g. "Tyrion").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The monitors belonging to this host.
    /// </summary>
    [JsonPropertyName("monitorList")]
    public List<KumaPublicMonitor> MonitorList { get; set; } = [];
}