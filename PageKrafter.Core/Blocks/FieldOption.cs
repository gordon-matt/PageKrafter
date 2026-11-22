namespace PageKrafter.Blocks;

/// <summary>One choice in a select field.</summary>
/// <param name="Value">The text stored in the prop.</param>
/// <param name="Label">The text shown in the drop-down.</param>
public sealed record FieldOption(string Value, string Label);