using System.Xml.Serialization;

namespace tzer0mApi.Models.Rss;

/// <summary>
/// The raw shape of a single entry in the RSS feed.
/// </summary>
public class RssItem
{
    /// <summary>
    /// The item's title.
    /// </summary>
    [XmlElement("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The item's full content, from the content:encoded element.
    /// </summary>
    [XmlElement("encoded", Namespace = "http://purl.org/rss/1.0/modules/content/")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the item was published, in the RFC 1123 format the feed publishes it in (e.g. "Wed, 16 Sep 2026 16:20:56 GMT").
    /// </summary>
    [XmlElement("pubDate")]
    public string PublishedAtRaw { get; set; } = string.Empty;
}