namespace PageKrafter.Components.Panels;

/// <summary>A schematic drawn from the template's block tree, so saved templates get a preview without screenshots.</summary>
public partial class TemplateThumbnail
{
    [Parameter, EditorRequired]
    public PageTemplate Template { get; set; } = default!;

    private IReadOnlyList<BlockNode> Rows => Template.Kind == TemplateKind.Page
        ? Template.Content.ChildrenOf(SlotDefinition.Default)
        : [Template.Content];
}
