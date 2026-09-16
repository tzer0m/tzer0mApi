using System.Text.Json;
using tzer0mApi.Models.Kuma;

namespace tzer0mApi.Services.Kuma;

/// <summary>
/// Fetches monitor status from Tom's Uptime Kuma public status page, for the e-ink display's status board.
/// </summary>
/// <param name="configuration">Configuration, used to resolve the Kuma base URL and status page slug.</param>
/// <param name="client">The HTTP client used to call Kuma's public status page API.</param>
public class KumaService(IConfiguration configuration, HttpClient client)
{
    /// <summary>
    /// How many consecutive checks are combined into one displayed history bucket - the last 100 checks become 25 buckets of this size.
    /// </summary>
    private const int RecentCheckGroupSize = 4;

    /// <summary>
    /// Options used to deserialize Kuma's JSON responses.
    /// </summary>
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Kuma's base URL, e.g. "https://kuma.tzer0m.co.uk".
    /// </summary>
    private string BaseUrl => configuration["Kuma:BaseUrl"] ?? throw new NullReferenceException(nameof(BaseUrl));

    /// <summary>
    /// The status page's slug, e.g. "default".
    /// </summary>
    private string StatusPageSlug => configuration["Kuma:StatusPageSlug"] ?? throw new NullReferenceException(nameof(StatusPageSlug));

    /// <summary>
    /// Gets the overall status and each host's aggregated status from Kuma's public status page.
    /// </summary>
    public async Task<KumaStatusSummary> GetStatusSummaryAsync()
    {
        KumaStatusPageResponse page = await GetAsync<KumaStatusPageResponse>($"{BaseUrl}/api/status-page/{StatusPageSlug}");
        KumaHeartbeatPageResponse heartbeats = await GetAsync<KumaHeartbeatPageResponse>($"{BaseUrl}/api/status-page/heartbeat/{StatusPageSlug}");

        Dictionary<int, int> latestStatusByMonitorId = [];
        foreach (KeyValuePair<string, List<KumaHeartbeatEntry>> entry in heartbeats.HeartbeatList)
            if (entry.Value.Count > 0 && int.TryParse(entry.Key, out int monitorId))
                latestStatusByMonitorId[monitorId] = entry.Value[^1].Status;

        List<KumaHostStatus> hosts = [.. page.PublicGroupList.Select(group => BuildHostStatus(group, heartbeats, latestStatusByMonitorId))];
        KumaOverallStatus overallStatus = ComputeOverallStatus(latestStatusByMonitorId.Values);
        return new KumaStatusSummary { OverallStatus = overallStatus, Hosts = hosts };
    }

    /// <summary>
    /// Builds one host's aggregated status from its monitors' latest heartbeats, 24-hour uptimes, and recent check history.
    /// </summary>
    /// <param name="group">The status page group representing the host.</param>
    /// <param name="heartbeats">The full heartbeat response, used to read each monitor's recent check history.</param>
    /// <param name="latestStatusByMonitorId">Each monitor's most recent heartbeat status, by monitor id.</param>
    private static KumaHostStatus BuildHostStatus(KumaPublicGroup group, KumaHeartbeatPageResponse heartbeats, Dictionary<int, int> latestStatusByMonitorId)
    {
        int upCount = group.MonitorList.Count(monitor => latestStatusByMonitorId.GetValueOrDefault(monitor.Id, -1) == (int)KumaHeartbeatStatus.Up);
        List<double> uptimeRatios = [.. group.MonitorList.Select(monitor => heartbeats.UptimeList.GetValueOrDefault($"{monitor.Id}_24", double.NaN)).Where(ratio => !double.IsNaN(ratio))];
        double uptimeRatio = uptimeRatios.Count > 0 ? uptimeRatios.Average() : 0d;
        List<List<KumaHeartbeatEntry>> monitorHeartbeats = [.. group.MonitorList.Select(monitor => heartbeats.HeartbeatList.GetValueOrDefault(monitor.Id.ToString(), [])).Where(heartbeatList => heartbeatList.Count > 0)];
        int checkCount = monitorHeartbeats.Count > 0 ? monitorHeartbeats.Min(heartbeatList => heartbeatList.Count) : 0;
        List<bool> checks = [];
        for (int checkIndex = 0; checkIndex < checkCount; checkIndex++)
            checks.Add(monitorHeartbeats.All(heartbeatList => heartbeatList[checkIndex].Status == (int)KumaHeartbeatStatus.Up));
        List<bool> recentCheckGroups = [];
        for (int groupStart = 0; groupStart < checks.Count; groupStart += RecentCheckGroupSize)
            recentCheckGroups.Add(checks.Skip(groupStart).Take(RecentCheckGroupSize).All(isUp => isUp));
        return new KumaHostStatus { Name = group.Name, UpCount = upCount, TotalCount = group.MonitorList.Count, UptimeRatio = uptimeRatio, RecentCheckGroups = recentCheckGroups };
    }

    /// <summary>
    /// Computes the overall status across every monitor, matching the logic Kuma's own status page uses - maintenance takes priority, then all-up, then partial, then all-down.
    /// </summary>
    /// <param name="latestStatuses">Every monitor's most recent heartbeat status.</param>
    private static KumaOverallStatus ComputeOverallStatus(IEnumerable<int> latestStatuses)
    {
        bool hasUp = false;
        bool hasNonUp = false;
        foreach (int status in latestStatuses)
        {
            if (status == (int)KumaHeartbeatStatus.Maintenance)
                return KumaOverallStatus.Maintenance;
            if (status == (int)KumaHeartbeatStatus.Up)
                hasUp = true;
            else
                hasNonUp = true;
        }
        if (!hasUp)
            return KumaOverallStatus.AllDown;
        return hasNonUp ? KumaOverallStatus.PartialDown : KumaOverallStatus.AllUp;
    }

    /// <summary>
    /// Sends a GET request to Kuma's REST API and deserializes the JSON response.
    /// </summary>
    /// <typeparam name="T">The response type to deserialize into.</typeparam>
    /// <param name="url">The full request URL.</param>
    private async Task<T> GetAsync<T>(string url)
    {
        string content = await client.GetStringAsync(url);
        return JsonSerializer.Deserialize<T>(content, SerializerOptions) ?? throw new InvalidOperationException($"Kuma returned an empty response from {url}");
    }
}