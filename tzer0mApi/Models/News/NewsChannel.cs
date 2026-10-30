using System.Xml.Serialization;

namespace tzer0mApi.Models.News;

/// <summary>
/// The raw shape of a news feed's channel - the list of stories it contains.
/// </summary>
public class NewsChannel
{
    /// <summary>
    /// The channel's stories, as published by the feed (top story first).
    /// </summary>
    [XmlElement("item")]
    public List<NewsItem> Items { get; set; } = [];
}