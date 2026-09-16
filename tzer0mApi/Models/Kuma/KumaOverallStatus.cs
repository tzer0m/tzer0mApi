namespace tzer0mApi.Models.Kuma;

/// <summary>
/// A Kuma status page's overall status, matching the states Kuma's own status page shows.
/// </summary>
public enum KumaOverallStatus
{
    /// <summary>
    /// Every monitor is up.
    /// </summary>
    AllUp,

    /// <summary>
    /// Some monitors are up and some are not.
    /// </summary>
    PartialDown,

    /// <summary>
    /// No monitor is up.
    /// </summary>
    AllDown,

    /// <summary>
    /// At least one monitor is under maintenance.
    /// </summary>
    Maintenance
}