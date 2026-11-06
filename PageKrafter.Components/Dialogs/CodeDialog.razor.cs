namespace PageKrafter.Components.Dialogs;

/// <summary>
/// Represents the code dialog.
/// </summary>
public partial class CodeDialog
{
    private bool open;
    private string html = string.Empty;

    /// <summary>
    /// Gets or sets the editor.
    /// </summary>
    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;

    internal void Open(string exported)
    {
        html = exported;
        open = true;
        StateHasChanged();
    }

    private void Close() => open = false;
}