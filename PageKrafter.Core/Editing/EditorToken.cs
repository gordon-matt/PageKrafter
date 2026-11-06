namespace PageKrafter.Editing;

/// <summary>
/// A placeholder a host wants authors to drop into an email. PageKrafter inserts <see cref="Value"/>
/// as text and does not interpret it, so the host can use Liquid, brackets, or anything else.
/// </summary>
/// <param name="Value">The text to insert, for example <c>{{ FirstName }}</c>.</param>
/// <param name="Description">Optional explanation shown beside the token.</param>
public sealed record EditorToken(string Value, string? Description = null);
