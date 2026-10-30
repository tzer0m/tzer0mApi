using System.Xml.Serialization;
using tzer0mApi.Models.News;

namespace tzer0mApi.Services.News;

/// <summary>
/// Fetches the news RSS feed and returns its top story.
/// </summary>
/// <param name="configuration">Configuration, used to resolve the feed's URL.</param>
/// <param name="client">The HTTP client used to fetch the feed.</param>
public class NewsService(IConfiguration configuration, HttpClient client)
{
    /// <summary>
    /// The news feed's URL, e.g. "https://feeds.bbci.co.uk/news/rss.xml".
    /// </summary>
    private string FeedUrl => configuration["News:FeedUrl"] ?? throw new NullReferenceException(nameof(FeedUrl));

    /// <summary>
    /// Fetches the feed and returns its top story.
    /// </summary>
    public async Task<NewsItem> GetTopStoryAsync()
    {
        string xml = await client.GetStringAsync(FeedUrl);
        XmlSerializer serializer = new(typeof(NewsFeed));
        using StringReader reader = new(xml);
        NewsFeed feed = serializer.Deserialize(reader) as NewsFeed ?? throw new InvalidOperationException($"Could not parse the news feed from {FeedUrl}");
        return feed.Channel.Items.FirstOrDefault() ?? throw new InvalidOperationException($"The news feed at {FeedUrl} contained no stories");
    }
}