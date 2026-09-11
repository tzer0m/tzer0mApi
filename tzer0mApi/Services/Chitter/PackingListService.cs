using System.Text;
using System.Text.Json;
using tzer0mApi.Models.Chitter;

namespace tzer0mApi.Services.Chitter;

/// <summary>
/// Loads the bundled packing list dataset and builds the printable checklist text from it.
/// </summary>
/// <param name="env">The web host environment, used to resolve the bundled data file's path.</param>
public class PackingListService(IWebHostEnvironment env)
{
    /// <summary>
    /// Options to make the json properties match the record properties.
    /// </summary>
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// The sections in the packing list, in print order. "International" and "Skiing" are conditional; every other section always prints.
    /// </summary>
    private readonly Lazy<List<PackingListSection>> Sections = new(() => LoadSections(env));

    /// <summary>
    /// Builds the printable packing list text: every unconditional section, plus International and/or Skiing when requested.
    /// </summary>
    /// <param name="includeInternational">Whether to include the International section.</param>
    /// <param name="includeSkiing">Whether to include the Skiing section.</param>
    public string BuildText(bool includeInternational, bool includeSkiing)
    {
        StringBuilder builder = new();
        foreach (PackingListSection section in Sections.Value)
        {
            if (section.Name.Equals("International", StringComparison.OrdinalIgnoreCase) && !includeInternational)
                continue;
            if (section.Name.Equals("Skiing", StringComparison.OrdinalIgnoreCase) && !includeSkiing)
                continue;

            if (builder.Length > 0)
                builder.Append('\n');

            builder.Append("**").Append(section.Name).Append("**").Append('\n');
            foreach (string item in section.Items)
                builder.Append("[ ] ").Append(item).Append('\n');
        }

        return builder.ToString().TrimEnd('\n');
    }

    /// <summary>
    /// Loads and deserializes the bundled packing list data file.
    /// </summary>
    /// <param name="env">The web host environment, used to resolve the file's path.</param>
    private static List<PackingListSection> LoadSections(IWebHostEnvironment env)
    {
        string path = Path.Combine(env.ContentRootPath, "Assets/Data/PackingList.json");
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<PackingListSection>>(json, SerializerOptions) ?? throw new InvalidOperationException("Assets/Data/PackingList.json deserialized to null");
    }
}