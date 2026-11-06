namespace PageKrafter.Blocks;

/// <summary>One setting in the inspector. The value is stored in <see cref="Documents.BlockNode.Props"/> under <see cref="Name"/>.</summary>
public sealed record BlockField
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Gets or sets the editor.
    /// </summary>
    public string Editor { get; init; } = FieldEditors.Text;

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public string DefaultValue { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the group.
    /// </summary>
    public string Group { get; init; } = FieldGroups.Content;

    /// <summary>
    /// Gets or sets the help text.
    /// </summary>
    public string? HelpText { get; init; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    public string? Placeholder { get; init; }

    /// <summary>
    /// Gets or sets the options.
    /// </summary>
    public IReadOnlyList<FieldOption> Options { get; init; } = [];

    /// <summary>
    /// The options come from <see cref="BlockDefinition.GetOptionsAsync"/> because they depend on host
    /// data (such as a list of CMS zones) or on other props of the block.
    /// </summary>
    public bool HasDynamicOptions { get; init; }

    /// <summary>Props that change the dynamic options. The inspector only reloads options when one of these changes.</summary>
    public IReadOnlyList<string> OptionsDependOn { get; init; } = [];

    /// <summary>
    /// Gets or sets the min.
    /// </summary>
    public decimal? Min { get; init; }

    /// <summary>
    /// Gets or sets the max.
    /// </summary>
    public decimal? Max { get; init; }

    /// <summary>
    /// Gets or sets the step.
    /// </summary>
    public decimal? Step { get; init; }

    /// <summary>
    /// Gets or sets the modes.
    /// </summary>
    public DocumentMode Modes { get; init; } = DocumentMode.All;

    /// <summary>
    /// Shows a Browse button on the field that opens the host's <see cref="Files.IFilePicker"/> for this kind of file.
    /// Image fields browse for images without setting this. It has no effect when the host registered no picker.
    /// </summary>
    public FilePickerKind? FilePicker { get; init; }

    /// <summary>
    /// When true, an email editor may insert host-supplied tokens into this field at the caret.
    /// The token text is inserted as-is. PageKrafter does not know how the host replaces it.
    /// </summary>
    public bool AcceptsTokens { get; init; }

    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="group">The group.</param>
    /// <returns>The text.</returns>
    public static BlockField Text(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group };

    /// <summary>
    /// Gets the text area.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="group">The group.</param>
    /// <returns>The text area.</returns>
    public static BlockField TextArea(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.TextArea };

    /// <summary>
    /// Gets the rich text.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The rich text.</returns>
    public static BlockField RichText(string name, string label, string defaultValue = "") =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Editor = FieldEditors.RichText };

    /// <summary>
    /// Gets the HTML.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The HTML.</returns>
    public static BlockField Html(string name, string label, string defaultValue = "") =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Editor = FieldEditors.Html };

    /// <summary>
    /// Gets the URL.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="group">The group.</param>
    /// <returns>The URL.</returns>
    public static BlockField Url(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.Url, Placeholder = "https://" };

    /// <summary>
    /// Gets the image.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="group">The group.</param>
    /// <returns>The image.</returns>
    public static BlockField Image(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.Image, Placeholder = "Image URL" };

    /// <summary>
    /// Gets the color.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="group">The group.</param>
    /// <returns>The color.</returns>
    public static BlockField Color(string name, string label, string defaultValue = "", string group = FieldGroups.Style) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.Color };

    /// <summary>
    /// Gets the checkbox.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="group">The group.</param>
    /// <returns>The checkbox.</returns>
    public static BlockField Checkbox(string name, string label, bool defaultValue = false, string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue ? "true" : "false", Group = group, Editor = FieldEditors.Checkbox };

    /// <summary>
    /// Gets the number.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="min">The min.</param>
    /// <param name="max">The max.</param>
    /// <param name="step">The step.</param>
    /// <param name="group">The group.</param>
    /// <returns>The number.</returns>
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

    /// <summary>
    /// Selects the value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="options">The options.</param>
    /// <returns>The block field.</returns>
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