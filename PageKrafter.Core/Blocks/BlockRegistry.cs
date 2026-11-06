using Microsoft.Extensions.Options;

namespace PageKrafter.Blocks;

/// <summary>
/// Represents the block registry.
/// </summary>
public sealed class BlockRegistry
{
    private readonly Dictionary<string, BlockDefinition> byType = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the BlockRegistry class.
    /// </summary>
    /// <param name="definitions">The definitions.</param>
    /// <param name="options">The options.</param>
    public BlockRegistry(IEnumerable<BlockDefinition> definitions, IOptions<PageKrafterOptions> options)
    {
        var builtInAssembly = typeof(BlockRegistry).Assembly;
        var disabled = options.Value.DisabledBlocks;

        // Host blocks are applied after built-ins whatever order they were registered in, so a host
        // can replace a built-in by reusing its type name.
        foreach (var definition in definitions.OrderBy(definition => definition.GetType().Assembly == builtInAssembly ? 0 : 1))
        {
            if (definition.Type == BlockTypes.Page || !disabled.Contains(definition.Type))
            {
                byType[definition.Type] = definition;
            }
        }
    }

    /// <summary>
    /// Gets the all.
    /// </summary>
    public IReadOnlyCollection<BlockDefinition> All => byType.Values;

    /// <summary>
    /// Finds the value.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The block definition.</returns>
    public BlockDefinition? Find(string? type) =>
        type is not null && byType.TryGetValue(type, out var definition) ? definition : null;

    /// <summary>
    /// Gets the palette for.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <returns>The palette for.</returns>
    public IReadOnlyList<BlockDefinition> PaletteFor(DocumentMode mode) => byType.Values
        .Where(definition => definition.ShowInPalette && definition.Modes.HasFlag(mode))
        .OrderBy(definition => BlockCategories.SortKey(definition.Category))
        .ThenBy(definition => definition.Category, StringComparer.OrdinalIgnoreCase)
        .ThenBy(definition => definition.Order)
        .ThenBy(definition => definition.Label, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    /// <summary>
    /// Gets the fields for.
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <param name="mode">The mode.</param>
    /// <returns>The fields for.</returns>
    public IReadOnlyList<BlockField> FieldsFor(BlockDefinition definition, DocumentMode mode)
    {
        var fields = definition.UsesCommonStyles ? definition.Fields.Concat(CommonStyles.Fields) : definition.Fields;
        return fields.Where(field => field.Modes.HasFlag(mode)).ToArray();
    }
}