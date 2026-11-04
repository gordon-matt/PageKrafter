namespace PageKrafter.Components.Dialogs;

public partial class CodeDialog
{
    private bool open;
    private string html = string.Empty;

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