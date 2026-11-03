using Microsoft.Extensions.Logging;

namespace PageKrafter.Components.Panels;

public partial class FieldInput
{
    private IReadOnlyList<FieldOption> loadedOptions = [];
    private string? optionsKey;
    private string? optionsError;
    private string? pickError;
    private IFilePicker? picker;

    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;

    [Parameter, EditorRequired]
    public BlockNode Node { get; set; } = default!;

    [Parameter, EditorRequired]
    public BlockDefinition Definition { get; set; } = default!;

    [Parameter, EditorRequired]
    public BlockField Field { get; set; } = default!;

    [Inject]
    private ILogger<FieldInput> Logger { get; set; } = default!;

    [Inject]
    private IServiceProvider Services { get; set; } = default!;

    private string InputId => $"pk-{Node.Id}-{Field.Name}";

    private string? Error => optionsError ?? pickError;

    private FilePickerKind? PickKind => Field.FilePicker ?? (Field.Editor == FieldEditors.Image ? FilePickerKind.Image : null);

    private bool CanBrowse => picker is not null && PickKind is not null;

    private string Current => Node.Get(Field.Name) ?? Field.DefaultValue;

    private Type? CustomEditor => Editor.Settings.FieldEditorComponents.TryGetValue(Field.Editor, out var type) ? type : null;

    private Dictionary<string, object?> CustomParameters => new()
    {
        [nameof(FieldEditorBase.Field)] = Field,
        [nameof(FieldEditorBase.Node)] = Node,
        [nameof(FieldEditorBase.Value)] = Current,
        [nameof(FieldEditorBase.ValueChanged)] = EventCallback.Factory.Create<string>(this, SetAsync),
    };

    private IReadOnlyList<FieldOption> Options
    {
        get
        {
            var options = Field.HasDynamicOptions ? loadedOptions : Field.Options;
            return Current.Length > 0 && options.All(option => option.Value != Current)
                ? [.. options, new FieldOption(Current, $"{Current} (not in the list)")]
                : options;
        }
    }

    private string ColorSwatch => Current.Length == 7 && Current[0] == '#' ? Current : "#000000";

    private string PreviewUrl => Html.SafeUrl(Current, allowDataImage: true);

    protected override void OnInitialized() => picker = Services.GetService<IFilePicker>();

    protected override async Task OnParametersSetAsync()
    {
        if (Field.Editor != FieldEditors.Select || !Field.HasDynamicOptions)
        {
            return;
        }

        string key = string.Join('|', Field.OptionsDependOn.Select(name => Node.Get(name)).Prepend(Node.Id));
        if (key == optionsKey)
        {
            return;
        }

        optionsKey = key;
        try
        {
            loadedOptions = await Definition.GetOptionsAsync(Field, Node);
            optionsError = null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogWarning(ex, "Options for {BlockType}.{FieldName} could not be loaded.", Definition.Type, Field.Name);
            loadedOptions = [];
            optionsError = "The options couldn't be loaded.";
        }
    }

    private Task SetAsync(string value) => Editor.SetPropAsync(Node.Id, Field.Name, value);

    private async Task BrowseAsync()
    {
        if (picker is null || PickKind is not { } kind)
        {
            return;
        }

        // Not guarded against a second click. Some file managers give no signal when they are closed
        // without a choice, so a click has to be able to start over.
        pickError = null;
        string nodeId = Node.Id;
        string fieldName = Field.Name;
        try
        {
            var picked = await picker.PickAsync(new FilePickRequest(kind, Field.Label, Current.Length > 0 ? Current : null));
            if (picked is not null)
            {
                await Editor.SetPropAsync(nodeId, fieldName, picked.Url);
            }
        }
        catch (Exception ex) when (ex is not (OperationCanceledException or JSDisconnectedException))
        {
            Logger.LogWarning(ex, "The file picker failed for {BlockType}.{FieldName}.", Definition.Type, fieldName);
            pickError = "The file picker couldn't be opened.";
        }
    }

    private Task OnInputAsync(ChangeEventArgs args) => SetAsync(args.Value?.ToString() ?? string.Empty);

    private Task OnChangeAsync(ChangeEventArgs args) => SetAsync(args.Value?.ToString() ?? string.Empty);

    private Task OnCheckedAsync(ChangeEventArgs args) => SetAsync(args.Value is true ? "true" : "false");
}
