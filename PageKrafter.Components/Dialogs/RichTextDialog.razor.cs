namespace PageKrafter.Components.Dialogs;

/// <summary>
/// Edits one rich text field with the Pickstack editor the host registered. The inspector is too
/// narrow for that editor, so it opens over the page instead.
/// </summary>
public partial class RichTextDialog
{
    private bool open;
    private string? nodeId;
    private string? prop;
    private string title = "Edit text";
    private int generation;

    /// <summary>The editor whose field is being changed. The HTML is written back as it changes, and rendering still sanitizes it.</summary>
    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;

    [Inject]
    private IFileRootProvider FileRoots { get; set; } = default!;

    private string? FileRoot => FileRoots.GetFileRoot();

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (open && (nodeId is null || Editor.Document.Find(nodeId) is null))
        {
            open = false;
        }
    }

    internal void Open(string nodeId, string prop, string label)
    {
        this.nodeId = nodeId;
        this.prop = prop;
        title = string.IsNullOrWhiteSpace(label) ? "Edit text" : $"Edit {label}";
        generation++;
        open = true;
        StateHasChanged();
    }

    private string Current
    {
        get
        {
            if (nodeId is null || prop is null)
            {
                return string.Empty;
            }

            var node = Editor.Document.Find(nodeId);
            if (node is null)
            {
                return string.Empty;
            }

            return node.Get(prop)
                ?? Editor.Blocks.Find(node.Type)?.Fields.FirstOrDefault(candidate => candidate.Name == prop)?.DefaultValue
                ?? string.Empty;
        }
    }

    private Task ChangedAsync(string? value) =>
        nodeId is null || prop is null || !open
            ? Task.CompletedTask
            : Editor.SetPropAsync(nodeId, prop, value ?? string.Empty);

    private void Close() => open = false;
}
