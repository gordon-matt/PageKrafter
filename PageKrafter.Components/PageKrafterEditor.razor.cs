using Ardalis.Result;
using Heroicsoft.NetForge.Components.Toasts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PageKrafter.Components.Dialogs;

namespace PageKrafter.Components;

/// <summary>
/// The page and email builder. Bind <see cref="Value"/> to the stored JSON; render pages from that
/// JSON with <see cref="PageRenderer"/>, or call <see cref="ExportHtmlAsync"/> to store HTML as well.
/// </summary>
public partial class PageKrafterEditor
{
    private static readonly EditorDevice[] Devices =
    [
        new("Desktop", "100%", Icons.Desktop),
        new("Tablet", "768px", Icons.Tablet),
        new("Mobile", "375px", Icons.Mobile),
    ];

    private ElementReference host;
    private ElementReference frame;
    private IJSObjectReference? module;
    private DotNetObjectReference<PageKrafterEditor>? reference;
    private bool initialized;
    private string? previousValue;
    private string? emittedJson;
    private EditorDevice device = Devices[0];
    private LeftTab leftTab = LeftTab.Blocks;
    private bool preview;
    private bool saving;
    private SaveTemplateDialog saveDialog = default!;
    private CodeDialog codeDialog = default!;
    private string? pendingCaretId;
    private int pendingCaret;

    /// <summary>The document as JSON. Empty starts a new document in <see cref="Mode"/>.</summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the value changed.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>The mode for a new document. Existing documents keep the mode stored in their JSON.</summary>
    [Parameter]
    public DocumentMode Mode { get; set; } = DocumentMode.Web;

    /// <summary>Shows a Save button that passes the current document to the host.</summary>
    [Parameter]
    public EventCallback<PageDocument> OnSave { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "80vh";

    /// <summary>Extra canvas stylesheets for this editor, added to <see cref="PageKrafterOptions.CanvasStyles"/>.</summary>
    [Parameter]
    public IReadOnlyList<string>? CanvasStyles { get; set; }

    /// <summary>Limits the palette to these block types, for example a simpler editor for a sidebar zone.</summary>
    [Parameter]
    public IReadOnlyCollection<string>? AllowedBlocks { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show templates.
    /// </summary>
    [Parameter]
    public bool ShowTemplates { get; set; } = true;

    /// <summary>Extra buttons for the toolbar, placed before Preview.</summary>
    [Parameter]
    public RenderFragment? ToolbarContent { get; set; }

    /// <summary>
    /// Placeholders offered while editing an email. Empty hides the token button.
    /// PageKrafter inserts each <see cref="EditorToken.Value"/> as text and does not know how the host replaces it.
    /// </summary>
    [Parameter]
    public IReadOnlyList<EditorToken>? Tokens { get; set; }

    [Inject]
    private BlockRegistry Registry { get; set; } = default!;

    [Inject]
    private PageRenderer Renderer { get; set; } = default!;

    [Inject]
    private ITemplateService TemplateService { get; set; } = default!;

    [Inject]
    private IHtmlContentSanitizer Sanitizer { get; set; } = default!;

    [Inject]
    private IOptions<PageKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    [Inject]
    private ILogger<PageKrafterEditor> Logger { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    /// <summary>
    /// Gets the document.
    /// </summary>
    public PageDocument Document => Session.Document;

    internal EditorSession Session { get; private set; } = default!;

    internal BlockRegistry Blocks => Registry;

    internal IReadOnlyList<PageTemplate> Templates { get; private set; } = [];

    internal PageKrafterOptions Settings => Options.Value;

    internal IReadOnlyList<EditorToken> TokenChoices =>
        Tokens is null
            ? []
            : Tokens.Where(token => token is not null && !string.IsNullOrWhiteSpace(token.Value)).ToArray();

    /// <summary>
    /// Exports the HTML.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the string.</returns>
    public Task<string> ExportHtmlAsync(CancellationToken cancellationToken = default) =>
        Renderer.RenderAsync(Session.Document, RenderOptions.Export, cancellationToken);

    /// <summary>
    /// Handles the canvas message event.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [JSInvokable]
    public async Task OnCanvasMessage(CanvasMessage message)
    {
        switch (message.Type)
        {
            case "select":
                Session.Select(message.Id);
                StateHasChanged();
                break;

            case "drop":
                await DropAsync(message);
                break;

            case "command":
                await RunCommandAsync(message.Command, message.Id);
                break;

            case "inline":
                await InlineEditAsync(message);
                break;
        }
    }

    /// <summary>
    /// Releases the resources used by this instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            try
            {
                await module.InvokeVoidAsync("dispose", frame);
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }

        reference?.Dispose();
    }

    internal Task InsertBlockAsync(string blockType) => ApplyAsync(Session.InsertNearSelection(blockType));

    internal Task InsertTemplateAsync(PageTemplate template) => ApplyAsync(Session.InsertTemplateNearSelection(template));

    internal Task SetPropAsync(string id, string name, string value) => ApplyAsync(Session.SetProp(id, name, value));

    internal async Task SelectAsync(string? id)
    {
        Session.Select(id);
        if (module is not null)
        {
            await module.InvokeVoidAsync("select", frame, Session.SelectedId);
        }

        StateHasChanged();
    }

    internal async Task RunCommandAsync(string? command, string? target)
    {
        switch (command)
        {
            case "undo":
                await UndoAsync();
                return;

            case "redo":
                await RedoAsync();
                return;

            case "deselect":
                await SelectAsync(null);
                return;
        }

        string? id = target ?? Session.SelectedId;
        if (id is null || id == Session.Document.Root.Id)
        {
            return;
        }

        switch (command)
        {
            case "delete":
                await ApplyAsync(Session.Delete(id));
                break;

            case "duplicate":
                await ApplyAsync(Session.Duplicate(id));
                break;

            case "up":
                await ApplyAsync(Session.MoveBy(id, -1));
                break;

            case "down":
                await ApplyAsync(Session.MoveBy(id, 1));
                break;

            case "parent":
                await SelectAsync(Session.ParentId(id));
                break;

            case "save-template":
                OpenSaveTemplate(TemplateKind.Section, id);
                break;
        }
    }

    internal async Task ApplyPageTemplateAsync(PageTemplate template)
    {
        if (await ConfirmAsync($"Replace the current page with \"{template.Name}\"? You can undo this."))
        {
            await ApplyAsync(Session.ApplyPageTemplate(template));
        }
    }

    internal void OpenSaveTemplate(TemplateKind kind, string? nodeId)
    {
        saveDialog.Open(kind, nodeId);
        StateHasChanged();
    }

    internal async Task<Result<PageTemplate>> SaveTemplateAsync(TemplateForm form, string? nodeId)
    {
        var content = nodeId is null ? Session.Document.Root : Session.Document.Find(nodeId);
        if (content is null)
        {
            return Result<PageTemplate>.NotFound();
        }

        var saved = await TemplateService.SaveAsync(form with { Mode = Session.Document.Mode }, content);
        if (saved.IsSuccess)
        {
            await LoadTemplatesAsync();
            Toasts.Success($"Saved \"{saved.Value.Name}\". Find it on the Templates tab.");
        }

        return saved;
    }

    internal async Task DeleteTemplateAsync(PageTemplate template)
    {
        if (!await ConfirmAsync($"Delete the template \"{template.Name}\"? Pages made from it are not affected."))
        {
            return;
        }

        var deleted = await TemplateService.DeleteAsync(template.Id);
        if (!deleted.IsSuccess)
        {
            Toasts.Error(deleted.ErrorMessage());
        }

        await LoadTemplatesAsync();
        StateHasChanged();
    }

    internal async Task CopyAsync(string text)
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("copyText", text);
            Toasts.Success("Copied to the clipboard.");
        }
    }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        if (!initialized)
        {
            initialized = true;
            previousValue = Value;
            Session = new EditorSession(Registry, Parse(Value), Options.Value.HistoryLimit, IsAllowed, TimeProvider);
            await LoadTemplatesAsync();
            return;
        }

        if (string.Equals(Value, previousValue, StringComparison.Ordinal))
        {
            return;
        }

        previousValue = Value;
        if (string.Equals(Value, emittedJson, StringComparison.Ordinal))
        {
            return;
        }

        var mode = Session.Document.Mode;
        Session.Load(Parse(Value));
        if (Session.Document.Mode != mode)
        {
            await LoadTemplatesAsync();
        }

        await LoadCanvasAsync();
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            reference = DotNetObjectReference.Create(this);
            module = await Js.InvokeAsync<IJSObjectReference>("import", CanvasDocument.ModuleUrl);
            await module.InvokeVoidAsync("mount", host, frame, reference);
            await LoadCanvasAsync();
        }

        if (pendingCaretId is not null && module is not null)
        {
            string elementId = pendingCaretId;
            int caret = pendingCaret;
            pendingCaretId = null;
            try
            {
                await module.InvokeVoidAsync("restoreCaret", elementId, caret);
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }

    internal async Task ReleaseTokenCaretAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("releaseTokenCaret", frame);
        }
        catch (JSDisconnectedException)
        {
        }
    }

    internal async Task InsertTokenAsync(EditorToken token)
    {
        if (module is null || string.IsNullOrEmpty(token.Value))
        {
            return;
        }

        TokenCaret? target;
        try
        {
            target = await module.InvokeAsync<TokenCaret?>("peekTokenTarget", frame);
        }
        catch (JSDisconnectedException)
        {
            return;
        }

        if (target is null)
        {
            Toasts.Warning("Click into the heading, text, or HTML first, then choose a token.");
            return;
        }

        if (string.Equals(target.Kind, "canvas", StringComparison.Ordinal))
        {
            bool inserted = await module.InvokeAsync<bool>("insertCanvasToken", frame, token.Value);
            if (inserted || await InsertIntoFieldAsync(target, token.Value))
            {
                return;
            }
        }
        else if (await InsertIntoFieldAsync(target, token.Value))
        {
            return;
        }

        Toasts.Warning("Click into the heading, text, or HTML first, then choose a token.");
    }

    private async Task<bool> InsertIntoFieldAsync(TokenCaret target, string token)
    {
        if (string.IsNullOrEmpty(target.NodeId) || string.IsNullOrEmpty(target.Prop))
        {
            return false;
        }

        var node = Session.Document.Find(target.NodeId);
        var field = node is null
            ? null
            : Registry.Find(node.Type)?.Fields.FirstOrDefault(candidate =>
                candidate.AcceptsTokens && string.Equals(candidate.Name, target.Prop, StringComparison.Ordinal));
        if (node is null || field is null)
        {
            return false;
        }

        // A rich-text caret is a position in the visible text, not in the stored HTML.
        if (string.Equals(target.Kind, "canvas", StringComparison.Ordinal) && field.Editor == FieldEditors.RichText)
        {
            return false;
        }

        string current = node.Get(field.Name) ?? field.DefaultValue;
        string updated = TokenInsertion.Insert(current, target.Start, target.End, token, out int caret);
        pendingCaretId = $"pk-{node.Id}-{field.Name}";
        pendingCaret = caret;
        await SetPropAsync(node.Id, field.Name, updated);
        return true;
    }

    private sealed class TokenCaret
    {
        public string Kind { get; set; } = string.Empty;

        public string NodeId { get; set; } = string.Empty;

        public string Prop { get; set; } = string.Empty;

        public int Start { get; set; }

        public int End { get; set; }
    }

    private bool IsAllowed(BlockDefinition definition) => AllowedBlocks is null || AllowedBlocks.Contains(definition.Type);

    private PageDocument Parse(string? json)
    {
        var parsed = PageKrafterJson.Deserialize(json, Mode);
        if (parsed.IsSuccess)
        {
            return parsed.Value;
        }

        Logger.LogWarning("Could not read the page data, so a new document was started: {Reason}", parsed.ErrorMessage());
        Toasts.Warning("The saved page couldn't be read, so a new one was started.");
        return PageDocument.Create(Mode);
    }

    private async Task LoadTemplatesAsync()
    {
        var listed = await TemplateService.ListAsync(Session.Document.Mode);
        if (listed.IsSuccess)
        {
            Templates = listed.Value;
        }
        else
        {
            Toasts.Error(listed.ErrorMessage());
        }
    }

    private async Task LoadCanvasAsync()
    {
        if (module is null)
        {
            return;
        }

        string body = await RenderCanvasAsync();
        var styles = Options.Value.CanvasStyles.Concat(CanvasStyles ?? []);
        string document = CanvasDocument.Build(body, Session.Document.Mode, Navigation.BaseUri, styles, Options.Value.CanvasHeadHtml);
        await module.InvokeVoidAsync("load", frame, document, Session.SelectedId, preview);
    }

    private async Task RefreshCanvasAsync()
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("render", frame, await RenderCanvasAsync(), Session.SelectedId);
        }
    }

    private async Task<string> RenderCanvasAsync()
    {
        try
        {
            return await Renderer.RenderAsync(Session.Document, RenderOptions.Editor);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "The page could not be rendered in the editor.");
            return $"<div class=\"pk-missing\">The page couldn't be shown: {Html.Encode(ex.Message)}</div>";
        }
    }

    private async Task ApplyAsync(IResult result)
    {
        if (!result.Succeeded())
        {
            Toasts.Error(result.ErrorMessage());
            StateHasChanged();
            return;
        }

        await DocumentChangedAsync();
    }

    private async Task DocumentChangedAsync()
    {
        emittedJson = PageKrafterJson.Serialize(Session.Document);
        await ValueChanged.InvokeAsync(emittedJson);
        await RefreshCanvasAsync();
        StateHasChanged();
    }

    private async Task DropAsync(CanvasMessage message)
    {
        if (message.ParentId is null || message.Slot is null)
        {
            return;
        }

        IResult result = message.Kind switch
        {
            "block" when message.BlockType is not null => Session.Insert(message.BlockType, message.ParentId, message.Slot, message.Index),
            "template" when Templates.FirstOrDefault(template => template.Id == message.TemplateId) is { } template =>
                Session.InsertTemplate(template, message.ParentId, message.Slot, message.Index),
            "move" when message.Id is not null => Session.Move(message.Id, message.ParentId, message.Slot, message.Index),
            _ => Result.Invalid(new ValidationError("There was nothing to drop.")),
        };

        await ApplyAsync(result);
    }

    private Task InlineEditAsync(CanvasMessage message)
    {
        if (message.Id is null || message.Prop is null)
        {
            return Task.CompletedTask;
        }

        string value = message.Kind == "rich"
            ? Sanitizer.Sanitize(message.Value ?? string.Empty)
            : string.Join(' ', (message.Value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        return ApplyAsync(Session.SetProp(message.Id, message.Prop, value));
    }

    private async Task UndoAsync()
    {
        if (Session.Undo())
        {
            await DocumentChangedAsync();
        }
    }

    private async Task RedoAsync()
    {
        if (Session.Redo())
        {
            await DocumentChangedAsync();
        }
    }

    private async Task TogglePreviewAsync()
    {
        preview = !preview;
        if (module is not null)
        {
            await module.InvokeVoidAsync("setPreview", frame, preview);
        }
    }

    private async Task ShowCodeAsync()
    {
        string html = Session.Document.Mode == DocumentMode.Email
            ? await Renderer.RenderDocumentAsync(Session.Document)
            : await Renderer.RenderAsync(Session.Document, RenderOptions.Export);

        codeDialog.Open(html);
    }

    private async Task SaveAsync()
    {
        saving = true;
        try
        {
            await OnSave.InvokeAsync(Session.Document);
        }
        finally
        {
            saving = false;
        }
    }

    private async Task<bool> ConfirmAsync(string message) =>
        module is null || await module.InvokeAsync<bool>("confirmAction", message);

    private enum LeftTab
    {
        Blocks,
        Templates,
        Layers,
    }

    private sealed record EditorDevice(string Label, string Width, MarkupString Icon);
}