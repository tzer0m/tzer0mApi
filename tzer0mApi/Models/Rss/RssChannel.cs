using System.Xml.Serialization;

namespace tzer0mApi.Models.Rss;

/// <summary>
/// The raw shape of an RSS feed's channel - its metadata plus the list of items it contains.
/// </summary>
public class RssChannel
{
    /// <summary>
    /// The channel's items, as published by the feed (newest first).
    /// </summary>
    [XmlElement("item")]
    public List<RssItem> Items { get; set; } = [];
}