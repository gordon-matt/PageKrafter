using System.Text.Json;

namespace PageKrafter.Components.Panels;

public partial class LayerNode
{
    private bool expanded = true;

    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;

    [Parameter, EditorRequired]
    public BlockNode Node { get; set; } = default!;

    [Parameter]
    public int Depth { get; set; }

    private BlockDefinition? Definition => Editor.Blocks.Find(Node.Type);

    private bool IsRoot => ReferenceEquals(Node, Editor.Session.Document.Root);

    private bool IsSelected => Node.Id == Editor.Session.SelectedId;

    private IReadOnlyList<SlotDefinition> Slots => Definition?.GetSlots(Node) ?? [];

    private bool HasChildren => Node.Slots.Values.Any(children => children.Count > 0);

    private string Icon => Definition?.Icon ?? BlockIcons.Square;

    private string Label => Definition?.Label ?? Node.Type;

    private string Summary => Definition?.Summarize(Node) ?? string.Empty;

    private string? MovePayload => IsRoot ? null : JsonSerializer.Serialize(new { kind = "move", id = Node.Id });

    private Task SelectAsync() => Editor.SelectAsync(Node.Id);

    private void Toggle() => expanded = !expanded;
}