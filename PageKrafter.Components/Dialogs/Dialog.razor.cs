using Microsoft.AspNetCore.Components.Web;

namespace PageKrafter.Components.Dialogs;

public partial class Dialog
{
    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Footer { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    private Task CloseAsync() => OnClose.InvokeAsync();

    private Task OnKeyDownAsync(KeyboardEventArgs args) => args.Key == "Escape" ? CloseAsync() : Task.CompletedTask;
}