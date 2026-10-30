using System.Xml.Serialization;

namespace tzer0mApi.Models.News;

/// <summary>
/// The raw root element of an RSS 2.0 news feed document.
/// </summary>
[XmlRoot("rss")]
public class NewsFeed
{
    /// <summary>
    /// The feed's single channel.
    /// </summary>
    [XmlElement("channel")]
    public NewsChannel Channel { get; set; } = new();
}