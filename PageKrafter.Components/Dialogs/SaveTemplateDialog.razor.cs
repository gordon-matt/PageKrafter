namespace PageKrafter.Components.Dialogs;

/// <summary>Asks for a name, category, and description, then saves the page or the selected section as a template.</summary>
public partial class SaveTemplateDialog
{
    private bool open;
    private bool saving;
    private TemplateKind kind;
    private string? nodeId;
    private string name = string.Empty;
    private string category = string.Empty;
    private string description = string.Empty;
    private string? error;

    /// <summary>The editor that owns the document being saved.</summary>
    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;

    private IEnumerable<string> Categories => Editor.Templates
        .Where(template => !template.IsReadOnly)
        .Select(template => template.Category)
        .Distinct(StringComparer.OrdinalIgnoreCase);

    internal void Open(TemplateKind templateKind, string? id)
    {
        kind = templateKind;
        nodeId = id;
        name = string.Empty;
        category = string.Empty;
        description = string.Empty;
        error = null;
        open = true;
        StateHasChanged();
    }

    private void Close() => open = false;

    private async Task SaveAsync()
    {
        saving = true;
        try
        {
            var saved = await Editor.SaveTemplateAsync(new TemplateForm
            {
                Name = name,
                Category = category,
                Description = description,
                Kind = kind,
            }, nodeId);

            if (saved.IsSuccess)
            {
                open = false;
            }
            else
            {
                error = saved.ErrorMessage();
            }
        }
        finally
        {
            saving = false;
        }
    }
}