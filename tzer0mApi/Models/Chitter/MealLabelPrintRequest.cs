namespace tzer0mApi.Models.Chitter;

/// <summary>
/// A request to print one or more meal labels - the same heading text on every label, with a distinct QR-coded guid per physical label.
/// </summary>
/// <param name="Name">The heading text printed on every label.</param>
/// <param name="Guids">The guid encoded as a QR code on each label, one physical label printed per entry, in order.</param>
public record MealLabelPrintRequest(string Name, List<Guid> Guids);