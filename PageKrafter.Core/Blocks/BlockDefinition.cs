namespace PageKrafter.Blocks;

/// <summary>
/// Describes a block type: its inspector fields, its child slots and how it renders. Register a
/// subclass with <c>AddPageKrafterBlock&lt;T&gt;()</c>. Definitions are resolved from DI per scope,
/// so they can inject host services (for example, a CMS zone service for dynamic options).
/// A host definition with the same <see cref="Type"/> as a built-in replaces the built-in.
/// </summary>
public abstract class BlockDefinition
{
    /// <summary>Unique key stored in the JSON. Prefix host blocks (for example <c>cms.content-zone</c>) so they never clash with built-ins.</summary>
    public abstract string Type { get; }

    public abstract string Label { get; }

    public virtual string Category => BlockCategories.Basic;

    /// <summary>Inline SVG markup or a short text glyph shown in the palette and the layers panel.</summary>
    public virtual string Icon => BlockIcons.Square;

    public virtual string Description => string.Empty;

    public virtual int Order => 100;

    public virtual bool ShowInPalette => true;

    /// <summary>Web only by default, because markup that works in a browser often breaks in email clients.</summary>
    public virtual DocumentMode Modes => DocumentMode.Web;

    /// <summary>Adds the shared padding, margin, colour, alignment, CSS class and visibility fields.</summary>
    public virtual bool UsesCommonStyles => true;

    public virtual IReadOnlyList<BlockField> Fields => [];

    /// <summary>
    /// A Razor component deriving from <c>BlockEditorBase</c> that replaces the generated content
    /// fields in the inspector. Style fields are still generated.
    /// </summary>
    public virtual Type? EditorComponent => null;

    public virtual IReadOnlyList<SlotDefinition> GetSlots(BlockNode node) => [];

    /// <summary>Common style props this block applies itself, so the renderer must not also add them to the root element.</summary>
    public virtual IReadOnlyCollection<string> SelfAppliedStyles(DocumentMode mode) => [];

    public virtual BlockNode Create()
    {
        var node = new BlockNode { Type = Type };
        foreach (var field in Fields.Where(field => field.DefaultValue.Length > 0))
        {
            node.Props[field.Name] = field.DefaultValue;
        }

        foreach (var slot in GetSlots(node))
        {
            node.Slot(slot.Name);
        }

        return node;
    }

    /// <summary>Called after a prop changes in the editor, so the block can keep related state consistent (for example, the number of column slots).</summary>
    public virtual void OnPropertyChanged(BlockNode node, string name)
    {
    }

    public virtual Task<IReadOnlyList<FieldOption>> GetOptionsAsync(BlockField field, BlockNode node, CancellationToken cancellationToken = default) =>
        Task.FromResult(field.Options);

    public virtual bool CanContain(BlockNode node, string slot, BlockDefinition child)
    {
        var definition = GetSlots(node).FirstOrDefault(candidate => string.Equals(candidate.Name, slot, StringComparison.Ordinal));
        return definition is not null
            && (definition.AllowedTypes is null || definition.AllowedTypes.Contains(child.Type, StringComparer.Ordinal));
    }

    public virtual bool CanBePlacedIn(BlockNode parent, string slot) => true;

    /// <summary>Short text shown next to the label in the layers panel so similar blocks can be told apart.</summary>
    public virtual string Summarize(BlockNode node) => string.Empty;

    /// <summary>
    /// Returns the block's HTML. Render a single root element: the renderer adds the common styles
    /// and, in the editor, the selection attributes to it. Call
    /// <see cref="BlockRenderContext.RenderSlotAsync"/> for child blocks and put
    /// <see cref="BlockRenderContext.SlotAttributes"/> on the element that contains them.
    /// </summary>
    public abstract ValueTask<string> RenderAsync(BlockRenderContext context);
}
