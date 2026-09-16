namespace tzer0mApi.Models.Kuma;

/// <summary>
/// The possible values of a Kuma heartbeat's status field.
/// </summary>
public enum KumaHeartbeatStatus
{
    /// <summary>
    /// The monitor was down at this check.
    /// </summary>
    Down = 0,

    /// <summary>
    /// The monitor was up at this check.
    /// </summary>
    Up = 1,

    /// <summary>
    /// The check is pending confirmation, e.g. within its retry window.
    /// </summary>
    Pending = 2,

    /// <summary>
    /// The monitor was under maintenance at this check.
    /// </summary>
    Maintenance = 3
}