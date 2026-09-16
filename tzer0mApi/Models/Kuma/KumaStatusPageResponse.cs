using System.Text.Json.Serialization;

namespace tzer0mApi.Models.Kuma;

/// <summary>
/// The raw shape of a status page's configuration, as returned by GET /api/status-page/{slug}.
/// </summary>
public class KumaStatusPageResponse
{
    /// <summary>
    /// The status page's groups, one per host.
    /// </summary>
    [JsonPropertyName("publicGroupList")]
    public List<KumaPublicGroup> PublicGroupList { get; set; } = [];
}