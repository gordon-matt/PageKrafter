namespace PageKrafter.Blocks;

/// <summary>One setting in the inspector. The value is stored in <see cref="Documents.BlockNode.Props"/> under <see cref="Name"/>.</summary>
public sealed record BlockField
{
    public required string Name { get; init; }

    public required string Label { get; init; }

    public string Editor { get; init; } = FieldEditors.Text;

    public string DefaultValue { get; init; } = string.Empty;

    public string Group { get; init; } = FieldGroups.Content;

    public string? HelpText { get; init; }

    public string? Placeholder { get; init; }

    public IReadOnlyList<FieldOption> Options { get; init; } = [];

    /// <summary>
    /// The options come from <see cref="BlockDefinition.GetOptionsAsync"/> because they depend on host
    /// data (such as a list of CMS zones) or on other props of the block.
    /// </summary>
    public bool HasDynamicOptions { get; init; }

    /// <summary>Props that change the dynamic options. The inspector only reloads options when one of these changes.</summary>
    public IReadOnlyList<string> OptionsDependOn { get; init; } = [];

    public decimal? Min { get; init; }

    public decimal? Max { get; init; }

    public decimal? Step { get; init; }

    public DocumentMode Modes { get; init; } = DocumentMode.All;

    /// <summary>
    /// Shows a Browse button on the field that opens the host's <see cref="Files.IFilePicker"/> for this kind of file.
    /// Image fields browse for images without setting this. It has no effect when the host registered no picker.
    /// </summary>
    public FilePickerKind? FilePicker { get; init; }

    public static BlockField Text(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group };

    public static BlockField TextArea(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.TextArea };

    public static BlockField RichText(string name, string label, string defaultValue = "") =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Editor = FieldEditors.RichText };

    public static BlockField Html(string name, string label, string defaultValue = "") =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Editor = FieldEditors.Html };

    public static BlockField Url(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.Url, Placeholder = "https://" };

    public static BlockField Image(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.Image, Placeholder = "Image URL" };

    public static BlockField Color(string name, string label, string defaultValue = "", string group = FieldGroups.Style) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.Color };

    public static BlockField Checkbox(string name, string label, bool defaultValue = false, string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue ? "true" : "false", Group = group, Editor = FieldEditors.Checkbox };

    public static BlockField Number(string name, string label, decimal defaultValue, decimal? min = null, decimal? max = null, decimal? step = null, string group = FieldGroups.Content) =>
        new()
        {
            Name = name,
            Label = label,
            DefaultValue = defaultValue.ToString(CultureInfo.InvariantCulture),
            Group = group,
            Editor = FieldEditors.Number,
            Min = min,
            Max = max,
            Step = step,
        };

    public static BlockField Select(string name, string label, string defaultValue, params (string Value, string Label)[] options) =>
        new()
        {
            Name = name,
            Label = label,
            DefaultValue = defaultValue,
            Editor = FieldEditors.Select,
            Options = options.Select(option => new FieldOption(option.Value, option.Label)).ToArray(),
        };
}