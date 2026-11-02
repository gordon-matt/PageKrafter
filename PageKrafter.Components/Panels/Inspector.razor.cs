namespace PageKrafter.Components.Panels;

public partial class Inspector
{
    private static readonly string[] GroupOrder = [FieldGroups.Content, FieldGroups.Style, FieldGroups.Advanced];

    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;

    private BlockNode? Node => Editor.Session.Selected;

    private BlockDefinition? Definition => Editor.Blocks.Find(Node?.Type);

    private bool IsRoot => Node is not null && ReferenceEquals(Node, Editor.Session.Document.Root);

    private IReadOnlyList<FieldGroupView> Groups
    {
        get
        {
            if (Definition is null)
            {
                return [];
            }

            bool custom = Definition.EditorComponent is not null;
            var fields = Editor.Blocks.FieldsFor(Definition, Editor.Session.Document.Mode)
                .Where(candidate => !custom || candidate.Group != FieldGroups.Content);

            var groups = fields
                .GroupBy(candidate => candidate.Group)
                .Select(group => new FieldGroupView(group.Key, group.ToArray(), custom && group.Key == FieldGroups.Content))
                .ToList();

            if (custom && groups.All(group => group.Name != FieldGroups.Content))
            {
                groups.Add(new FieldGroupView(FieldGroups.Content, [], true));
            }

            return groups
                .OrderBy(group => Array.IndexOf(GroupOrder, group.Name) is int index and >= 0 ? index : GroupOrder.Length)
                .ThenBy(group => group.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    private Dictionary<string, object?> CustomEditorParameters => new()
    {
        [nameof(BlockEditorBase.Node)] = Node,
        [nameof(BlockEditorBase.Definition)] = Definition,
        [nameof(BlockEditorBase.OnChange)] = EventCallback.Factory.Create<PropertyChange>(this, change => Editor.SetPropAsync(Node!.Id, change.Name, change.Value)),
    };

    private sealed record FieldGroupView(string Name, IReadOnlyList<BlockField> Fields, bool UseCustomEditor);
}
