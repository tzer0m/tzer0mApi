using System.Text.Json;
using tzer0mApi.Models.Chitter;

namespace tzer0mApi.Services.Chitter;

/// <summary>
/// Loads the bundled motivational quote and Chinese proverb datasets and hands back random picks.
/// </summary>
/// <param name="env">The web host environment, used to resolve the bundled data files' paths.</param>
public class QuoteService(IWebHostEnvironment env)
{
    /// <summary>
    /// Options to make the json properties match the record properties.
    /// </summary>
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// List of motivational quotes.
    /// </summary>
    private readonly Lazy<List<MotivationalQuote>> Quotes = new(() => LoadData<MotivationalQuote>(env, "Assets/Data/MotivationalQuotes.json"));

    /// <summary>
    /// List of chinese proverbs.
    /// </summary>
    private readonly Lazy<List<ChineseProverb>> Proverbs = new(() => LoadData<ChineseProverb>(env, "Assets/Data/ChineseProverbs.json"));

    /// <summary>
    /// Returns a random motivational quote.
    /// </summary>
    public MotivationalQuote GetRandomQuote() => Quotes.Value[Random.Shared.Next(Quotes.Value.Count)];

    /// <summary>
    /// Returns a random Chinese proverb.
    /// </summary>
    public ChineseProverb GetRandomProverb() => Proverbs.Value[Random.Shared.Next(Proverbs.Value.Count)];

    /// <summary>
    /// Loads and deserializes a bundled JSON data file.
    /// </summary>
    /// <typeparam name="T">The record type to deserialize each entry into.</typeparam>
    /// <param name="env">The web host environment, used to resolve the file's path.</param>
    /// <param name="relativePath">The data file's path, relative to the content root.</param>
    private static List<T> LoadData<T>(IWebHostEnvironment env, string relativePath)
    {
        string path = Path.Combine(env.ContentRootPath, relativePath);
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<T>>(json, SerializerOptions) ?? throw new InvalidOperationException($"{relativePath} deserialized to null");
    }
}