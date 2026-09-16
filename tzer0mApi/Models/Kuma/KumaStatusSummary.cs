namespace tzer0mApi.Models.Kuma;

/// <summary>
/// The full status board for display B - the overall status banner plus each host's row.
/// </summary>
public class KumaStatusSummary
{
    /// <summary>
    /// The overall status shown at the top of the board, matching Kuma's own status page logic.
    /// </summary>
    public KumaOverallStatus OverallStatus { get; set; }

    /// <summary>
    /// Each host's row, in status page order.
    /// </summary>
    public List<KumaHostStatus> Hosts { get; set; } = [];
}