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

    /// <summary>The name shown in the palette, the inspector, and the layers panel.</summary>
    public abstract string Label { get; }

    /// <summary>The palette group. The default is <see cref="BlockCategories.Basic"/>.</summary>
    public virtual string Category => BlockCategories.Basic;

    /// <summary>Inline SVG markup or a short text glyph shown in the palette and the layers panel.</summary>
    public virtual string Icon => BlockIcons.Square;

    /// <summary>A short description under the palette item. Empty shows nothing.</summary>
    public virtual string Description => string.Empty;

    /// <summary>Sort order inside the category. Lower comes first. The default is 100.</summary>
    public virtual int Order => 100;

    /// <summary><see langword="false"/> hides the block from the palette. It can still be rendered and selected. The default is <see langword="true"/>.</summary>
    public virtual bool ShowInPalette => true;

    /// <summary>Web only by default, because markup that works in a browser often breaks in email clients.</summary>
    public virtual DocumentMode Modes => DocumentMode.Web;

    /// <summary>Adds the shared padding, margin, colour, alignment, CSS class and visibility fields.</summary>
    public virtual bool UsesCommonStyles => true;

    /// <summary>Inspector fields for this block. Empty means only the common style fields, when those are enabled.</summary>
    public virtual IReadOnlyList<BlockField> Fields => [];

    /// <summary>
    /// A Razor component deriving from <c>BlockEditorBase</c> that replaces the generated content
    /// fields in the inspector. Style fields are still generated.
    /// </summary>
    public virtual Type? EditorComponent => null;

    /// <summary>The child areas of this block. The default is none.</summary>
    /// <param name="node">The block being edited. A block can change its slots from the node's props.</param>
    /// <returns>The slots.</returns>
    public virtual IReadOnlyList<SlotDefinition> GetSlots(BlockNode node) => [];

    /// <summary>Common style props this block applies itself, so the renderer must not also add them to the root element.</summary>
    public virtual IReadOnlyCollection<string> SelfAppliedStyles(DocumentMode mode) => [];

    /// <summary>A new block of this type. Props that have a default are set, and each slot is created empty.</summary>
    /// <returns>The block.</returns>
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

    /// <summary>The choices for a select field. The default is <see cref="BlockField.Options"/>. Override this when <see cref="BlockField.HasDynamicOptions"/> is set.</summary>
    /// <param name="field">The select field.</param>
    /// <param name="node">The block whose other props may decide the choices.</param>
    /// <param name="cancellationToken">A token that can cancel the lookup.</param>
    /// <returns>The choices.</returns>
    public virtual Task<IReadOnlyList<FieldOption>> GetOptionsAsync(BlockField field, BlockNode node, CancellationToken cancellationToken = default) =>
        Task.FromResult(field.Options);

    /// <summary>
    /// Whether <paramref name="child"/> may be placed in <paramref name="slot"/>.
    /// A missing slot refuses it. A slot with <see cref="SlotDefinition.AllowedTypes"/> refuses anything not in that list.
    /// </summary>
    /// <param name="node">This block.</param>
    /// <param name="slot">The slot name.</param>
    /// <param name="child">The block being placed.</param>
    /// <returns><see langword="true"/> when the child is allowed.</returns>
    public virtual bool CanContain(BlockNode node, string slot, BlockDefinition child)
    {
        var definition = GetSlots(node).FirstOrDefault(candidate => string.Equals(candidate.Name, slot, StringComparison.Ordinal));
        return definition is not null
            && (definition.AllowedTypes is null || definition.AllowedTypes.Contains(child.Type, StringComparer.Ordinal));
    }

    /// <summary>Whether this block may be placed in the parent's slot. The default allows every parent.</summary>
    /// <param name="parent">The block that would contain this one.</param>
    /// <param name="slot">The slot name.</param>
    /// <returns><see langword="true"/> when the placement is allowed.</returns>
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