using System.Globalization;
using System.Xml.Serialization;
using tzer0mApi.Models.Rss;

namespace tzer0mApi.Services.Rss;

/// <summary>
/// Fetches and parses Tom's RSS feed, for the e-ink display's feed panel.
/// </summary>
/// <param name="configuration">Configuration, used to resolve the feed's base URL.</param>
/// <param name="client">The HTTP client used to fetch the feed.</param>
public class RssService(IConfiguration configuration, HttpClient client)
{
    /// <summary>
    /// The RFC 1123 format the feed publishes each item's date in.
    /// </summary>
    private const string PublishedAtFormat = "R";

    /// <summary>
    /// The feed's base URL, e.g. "https://rss.tzer0m.co.uk".
    /// </summary>
    private string BaseUrl => configuration["Rss:BaseUrl"] ?? throw new NullReferenceException(nameof(BaseUrl));

    /// <summary>
    /// Fetches the feed and returns its items, newest first.
    /// </summary>
    public async Task<List<RssFeedItem>> GetItemsAsync()
    {
        string xml = await client.GetStringAsync(BaseUrl);
        XmlSerializer serializer = new(typeof(RssFeed));
        using StringReader reader = new(xml);
        RssFeed feed = serializer.Deserialize(reader) as RssFeed ?? throw new InvalidOperationException($"Could not parse the RSS feed from {BaseUrl}");
        return [.. feed.Channel.Items.Select(ToFeedItem)];
    }

    /// <summary>
    /// Converts a raw feed item into its display-ready form, parsing its published date.
    /// </summary>
    /// <param name="item">The raw feed item.</param>
    private static RssFeedItem ToFeedItem(RssItem item)
    {
        DateTime publishedAt = DateTime.TryParseExact(item.PublishedAtRaw, PublishedAtFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime parsed) ? parsed : DateTime.UtcNow;
        return new RssFeedItem { PublishedAt = publishedAt, Title = item.Title, Content = item.Content };
    }
}