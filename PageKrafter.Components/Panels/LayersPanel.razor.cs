namespace PageKrafter.Components.Panels;

/// <summary>
/// Represents the layers panel.
/// </summary>
public partial class LayersPanel
{
    /// <summary>
    /// Gets or sets the editor.
    /// </summary>
    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;
}