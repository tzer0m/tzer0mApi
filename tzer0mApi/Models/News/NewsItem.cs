using System.Xml.Serialization;

namespace tzer0mApi.Models.News;

/// <summary>
/// A single news story - its headline and one-line summary.
/// </summary>
public class NewsItem
{
    /// <summary>
    /// The story's headline.
    /// </summary>
    [XmlElement("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The story's one-line summary.
    /// </summary>
    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;
}