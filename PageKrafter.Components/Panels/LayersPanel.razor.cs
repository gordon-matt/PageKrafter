namespace PageKrafter.Components.Panels;

public partial class LayersPanel
{
    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;
}
