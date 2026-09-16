namespace tzer0mApi.Models.Kuma;

/// <summary>
/// A single host's aggregated status, combining every monitor Kuma groups under that host.
/// </summary>
public class KumaHostStatus
{
    /// <summary>
    /// The host's name (the status page group name).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// How many of the host's monitors are currently up.
    /// </summary>
    public int UpCount { get; set; }

    /// <summary>
    /// How many monitors Kuma tracks for this host.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The host's average 24-hour uptime across its monitors, as a 0-1 ratio.
    /// </summary>
    public double UptimeRatio { get; set; }

    /// <summary>
    /// Recent check history across the host's monitors, oldest first, grouped into buckets of a few checks each - a bucket is true only if every check in it, across every monitor, was up (the worst case within the bucket wins).
    /// </summary>
    public List<bool> RecentCheckGroups { get; set; } = [];
}