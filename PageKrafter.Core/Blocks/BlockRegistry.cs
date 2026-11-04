using Microsoft.Extensions.Options;

namespace PageKrafter.Blocks;

public sealed class BlockRegistry
{
    private readonly Dictionary<string, BlockDefinition> byType = new(StringComparer.Ordinal);

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

    public IReadOnlyCollection<BlockDefinition> All => byType.Values;

    public BlockDefinition? Find(string? type) =>
        type is not null && byType.TryGetValue(type, out var definition) ? definition : null;

    public IReadOnlyList<BlockDefinition> PaletteFor(DocumentMode mode) => byType.Values
        .Where(definition => definition.ShowInPalette && definition.Modes.HasFlag(mode))
        .OrderBy(definition => BlockCategories.SortKey(definition.Category))
        .ThenBy(definition => definition.Category, StringComparer.OrdinalIgnoreCase)
        .ThenBy(definition => definition.Order)
        .ThenBy(definition => definition.Label, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public IReadOnlyList<BlockField> FieldsFor(BlockDefinition definition, DocumentMode mode)
    {
        var fields = definition.UsesCommonStyles ? definition.Fields.Concat(CommonStyles.Fields) : definition.Fields;
        return fields.Where(field => field.Modes.HasFlag(mode)).ToArray();
    }
}