namespace tzer0mApi.Models.Chitter;

/// <summary>
/// A motivational quote and its author.
/// </summary>
/// <param name="Quote">The quote text.</param>
/// <param name="Author">The person credited with the quote.</param>
public record MotivationalQuote(string Quote, string Author);