using Heroicsoft.NetForge.Components.Toasts;
using PageKrafter.Demo.Storage;

namespace PageKrafter.Demo.Components.Pages;

public partial class BuilderPage
{
    private string? json;
    private bool loaded;
    private string? status;

    [Inject]
    private DemoPageStore Pages { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    [Parameter, EditorRequired]
    public string Name { get; set; } = default!;

    [Parameter]
    public DocumentMode Mode { get; set; } = DocumentMode.Web;

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override async Task OnInitializedAsync()
    {
        json = await Pages.LoadAsync(Name);
        loaded = true;
    }

    private async Task SaveAsync(PageDocument document)
    {
        await Pages.SaveAsync(Name, document);
        status = $"Saved at {DateTime.Now:HH:mm:ss}";
        Toasts.Success("Saved.");
    }
}
