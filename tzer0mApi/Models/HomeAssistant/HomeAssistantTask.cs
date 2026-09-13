namespace tzer0mApi.Models.HomeAssistant;

/// <summary>
/// A single task pulled from Home Assistant that is overdue or due today.
/// </summary>
public class HomeAssistantTask
{
    /// <summary>
    /// The task's title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The task's due date.
    /// </summary>
    public DateOnly Due { get; set; }

    /// <summary>
    /// Whether the task is overdue or due today.
    /// </summary>
    public HomeAssistantTaskStatus Status { get; set; }
}