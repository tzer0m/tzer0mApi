using System.Xml.Serialization;

namespace tzer0mApi.Models.Rss;

/// <summary>
/// The raw root element of an RSS 2.0 feed document, as returned by GET https://rss.tzer0m.co.uk.
/// </summary>
[XmlRoot("rss")]
public class RssFeed
{
    /// <summary>
    /// The feed's single channel.
    /// </summary>
    [XmlElement("channel")]
    public RssChannel Channel { get; set; } = new();
}