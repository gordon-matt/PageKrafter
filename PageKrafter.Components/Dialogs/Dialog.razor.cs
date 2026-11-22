using Microsoft.AspNetCore.Components.Web;

namespace PageKrafter.Components.Dialogs;

/// <summary>A dialog over the editor. Escape and the close callback both dismiss it.</summary>
public partial class Dialog
{
    /// <summary><see langword="true"/> shows the dialog.</summary>
    [Parameter]
    public bool Open { get; set; }

    /// <summary>The heading.</summary>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gives the dialog most of the editor, for a rich text editor that does not fit the inspector.</summary>
    [Parameter]
    public bool Wide { get; set; }

    /// <summary>The dialog body.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Buttons under the body. <see langword="null"/> shows none.</summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>Raised when the editor dismisses the dialog.</summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    private Task CloseAsync() => OnClose.InvokeAsync();

    private Task OnKeyDownAsync(KeyboardEventArgs args) => args.Key == "Escape" ? CloseAsync() : Task.CompletedTask;
}