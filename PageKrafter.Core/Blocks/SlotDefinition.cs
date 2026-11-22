namespace PageKrafter.Blocks;

/// <summary>A named area inside a block that holds child blocks, such as a section body or one column.</summary>
/// <param name="Name">The slot key stored in the JSON.</param>
/// <param name="Label">The name shown when the slot is empty in the editor.</param>
public sealed record SlotDefinition(string Name, string Label)
{
    /// <summary>The slot used by blocks that have one child area. The value is <c>default</c>.</summary>
    public const string Default = "default";

    /// <summary>When set, only these block types can be dropped into the slot.</summary>
    public IReadOnlyList<string>? AllowedTypes { get; init; }
}