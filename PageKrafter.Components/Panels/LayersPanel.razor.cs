namespace PageKrafter.Components.Panels;

/// <summary>The document as a tree, starting at the page. Selecting a row selects that block on the canvas.</summary>
public partial class LayersPanel
{
    /// <summary>The editor whose document this panel shows.</summary>
    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;
}