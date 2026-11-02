namespace PageKrafter.Blocks;

/// <summary>A named area inside a block that holds child blocks, such as a section body or one column.</summary>
public sealed record SlotDefinition(string Name, string Label)
{
    public const string Default = "default";

    /// <summary>When set, only these block types can be dropped into the slot.</summary>
    public IReadOnlyList<string>? AllowedTypes { get; init; }
}
