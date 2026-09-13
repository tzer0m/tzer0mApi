namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// Where a task sits relative to today, for display grouping.
/// </summary>
public enum HomeAssistantTaskStatus
{
    /// <summary>
    /// The task's due date has already passed.
    /// </summary>
    Overdue,

    /// <summary>
    /// The task is due today.
    /// </summary>
    DueToday
}