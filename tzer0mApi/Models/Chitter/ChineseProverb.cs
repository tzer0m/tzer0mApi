namespace tzer0mApi.Models.Chitter;

/// <summary>
/// A Chinese proverb and its English translation.
/// </summary>
/// <param name="Chinese">The proverb in Chinese.</param>
/// <param name="English">The English translation.</param>
public record ChineseProverb(string Chinese, string English);