using Microsoft.AspNetCore.Components.Web;

namespace PageKrafter.Components.Dialogs;

/// <summary>
/// Represents the dialog.
/// </summary>
public partial class Dialog
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is open.
    /// </summary>
    [Parameter]
    public bool Open { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the footer.
    /// </summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>
    /// Gets or sets the callback raised for the close event.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    private Task CloseAsync() => OnClose.InvokeAsync();

    private Task OnKeyDownAsync(KeyboardEventArgs args) => args.Key == "Escape" ? CloseAsync() : Task.CompletedTask;
}