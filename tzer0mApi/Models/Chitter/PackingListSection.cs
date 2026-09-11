namespace tzer0mApi.Models.Chitter;

/// <summary>
/// One named section of the packing list (e.g. "Clothing"), and the items in it.
/// </summary>
public record PackingListSection(string Name, List<string> Items);