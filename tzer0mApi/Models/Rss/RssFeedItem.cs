namespace tzer0mApi.Models.Rss;

/// <summary>
/// A single RSS feed entry, parsed and ready to display.
/// </summary>
public class RssFeedItem
{
    /// <summary>
    /// When the item was published, in UTC.
    /// </summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// The item's title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The item's full content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}