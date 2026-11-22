namespace PageKrafter.Components.Dialogs;

/// <summary>Shows the HTML the current document exports. An email is a complete document. A web page is the body.</summary>
public partial class CodeDialog
{
    private bool open;
    private string html = string.Empty;

    /// <summary>The editor the dialog belongs to.</summary>
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