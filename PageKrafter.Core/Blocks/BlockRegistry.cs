using Microsoft.Extensions.Options;

namespace PageKrafter.Blocks;

/// <summary>The block types available to the editor and the renderer. One instance is created per scope.</summary>
public sealed class BlockRegistry
{
    private readonly Dictionary<string, BlockDefinition> byType = new(StringComparer.Ordinal);

    /// <summary>
    /// Builds the registry. Built-in definitions are applied first, then host definitions, so a host block that reuses a type name replaces the built-in.
    /// A type in <see cref="PageKrafterOptions.DisabledBlocks"/> is left out. The page block is always kept.
    /// </summary>
    /// <param name="definitions">Every registered <see cref="BlockDefinition"/>.</param>
    /// <param name="options">The options, including the disabled types.</param>
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

    /// <summary>Every block that survived the disabled list, including the page block.</summary>
    public IReadOnlyCollection<BlockDefinition> All => byType.Values;

    /// <summary>The definition for a type key.</summary>
    /// <param name="type">The type key. <see langword="null"/> matches nothing.</param>
    /// <returns>The definition, or <see langword="null"/> when the type is unknown or disabled.</returns>
    public BlockDefinition? Find(string? type) =>
        type is not null && byType.TryGetValue(type, out var definition) ? definition : null;

    /// <summary>
    /// Palette items for a document mode. Hidden blocks and blocks that do not include the mode are left out.
    /// Layout, Basic, Media, and Advanced come first, then each category's <see cref="BlockDefinition.Order"/>, then the label.
    /// </summary>
    /// <param name="mode">The document mode.</param>
    /// <returns>The blocks, in palette order.</returns>
    public IReadOnlyList<BlockDefinition> PaletteFor(DocumentMode mode) => byType.Values
        .Where(definition => definition.ShowInPalette && definition.Modes.HasFlag(mode))
        .OrderBy(definition => BlockCategories.SortKey(definition.Category))
        .ThenBy(definition => definition.Category, StringComparer.OrdinalIgnoreCase)
        .ThenBy(definition => definition.Order)
        .ThenBy(definition => definition.Label, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    /// <summary>
    /// Inspector fields for a block. Common style fields come first when the block uses them, then the block's own fields.
    /// A field whose modes do not include <paramref name="mode"/> is left out.
    /// </summary>
    /// <param name="definition">The block.</param>
    /// <param name="mode">The document mode.</param>
    /// <returns>The fields, in inspector order.</returns>
    public IReadOnlyList<BlockField> FieldsFor(BlockDefinition definition, DocumentMode mode)
    {
        var fields = definition.UsesCommonStyles ? definition.Fields.Concat(CommonStyles.Fields) : definition.Fields;
        return fields.Where(field => field.Modes.HasFlag(mode)).ToArray();
    }
}