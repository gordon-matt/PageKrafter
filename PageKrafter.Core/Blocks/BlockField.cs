namespace PageKrafter.Blocks;

/// <summary>One setting in the inspector. The value is stored in <see cref="Documents.BlockNode.Props"/> under <see cref="Name"/>.</summary>
public sealed record BlockField
{
    /// <summary>The prop name stored in the JSON.</summary>
    public required string Name { get; init; }

    /// <summary>The label shown beside the field.</summary>
    public required string Label { get; init; }

    /// <summary>Which input draws the field. A built-in name from <see cref="FieldEditors"/>, or a name registered with a custom editor. The default is a text box.</summary>
    public string Editor { get; init; } = FieldEditors.Text;

    /// <summary>The value used when the prop has never been set. A new block stores this when it is not empty.</summary>
    public string DefaultValue { get; init; } = string.Empty;

    /// <summary>The inspector group. The default is <see cref="FieldGroups.Content"/>.</summary>
    public string Group { get; init; } = FieldGroups.Content;

    /// <summary>Text shown under the field. <see langword="null"/> shows nothing.</summary>
    public string? HelpText { get; init; }

    /// <summary>Placeholder text for an empty box. <see langword="null"/> shows none.</summary>
    public string? Placeholder { get; init; }

    /// <summary>The choices for a select field. Ignored when <see cref="HasDynamicOptions"/> is set.</summary>
    public IReadOnlyList<FieldOption> Options { get; init; } = [];

    /// <summary>
    /// The options come from <see cref="BlockDefinition.GetOptionsAsync"/> because they depend on host
    /// data (such as a list of CMS zones) or on other props of the block.
    /// </summary>
    public bool HasDynamicOptions { get; init; }

    /// <summary>Props that change the dynamic options. The inspector only reloads options when one of these changes.</summary>
    public IReadOnlyList<string> OptionsDependOn { get; init; } = [];

    /// <summary>The smallest value for a number field. <see langword="null"/> leaves it unset.</summary>
    public decimal? Min { get; init; }

    /// <summary>The largest value for a number field. <see langword="null"/> leaves it unset.</summary>
    public decimal? Max { get; init; }

    /// <summary>The step for a number field. <see langword="null"/> leaves it unset.</summary>
    public decimal? Step { get; init; }

    /// <summary>Which document modes show this field. The default is both.</summary>
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

    /// <summary>A single-line text field.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The value stored on a new block. The default is empty.</param>
    /// <param name="group">The inspector group. The default is Content.</param>
    /// <returns>The field.</returns>
    public static BlockField Text(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group };

    /// <summary>A multi-line text field.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The value stored on a new block. The default is empty.</param>
    /// <param name="group">The inspector group. The default is Content.</param>
    /// <returns>The field.</returns>
    public static BlockField TextArea(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.TextArea };

    /// <summary>A rich text field in the Content group.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The HTML stored on a new block. The default is empty.</param>
    /// <returns>The field.</returns>
    public static BlockField RichText(string name, string label, string defaultValue = "") =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Editor = FieldEditors.RichText };

    /// <summary>A raw HTML field in the Content group.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The markup stored on a new block. The default is empty.</param>
    /// <returns>The field.</returns>
    public static BlockField Html(string name, string label, string defaultValue = "") =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Editor = FieldEditors.Html };

    /// <summary>A URL field. The placeholder is <c>https://</c>.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The URL stored on a new block. The default is empty.</param>
    /// <param name="group">The inspector group. The default is Content.</param>
    /// <returns>The field.</returns>
    public static BlockField Url(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.Url, Placeholder = "https://" };

    /// <summary>An image URL field. It browses for images when a file picker is registered. The placeholder is "Image URL".</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The URL stored on a new block. The default is empty.</param>
    /// <param name="group">The inspector group. The default is Content.</param>
    /// <returns>The field.</returns>
    public static BlockField Image(string name, string label, string defaultValue = "", string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.Image, Placeholder = "Image URL" };

    /// <summary>A color field. The default group is Style.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The color stored on a new block. The default is empty.</param>
    /// <param name="group">The inspector group. The default is Style.</param>
    /// <returns>The field.</returns>
    public static BlockField Color(string name, string label, string defaultValue = "", string group = FieldGroups.Style) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue, Group = group, Editor = FieldEditors.Color };

    /// <summary>A checkbox. The stored value is <c>true</c> or <c>false</c>.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue"><see langword="true"/> stores <c>true</c> on a new block.</param>
    /// <param name="group">The inspector group. The default is Content.</param>
    /// <returns>The field.</returns>
    public static BlockField Checkbox(string name, string label, bool defaultValue = false, string group = FieldGroups.Content) =>
        new() { Name = name, Label = label, DefaultValue = defaultValue ? "true" : "false", Group = group, Editor = FieldEditors.Checkbox };

    /// <summary>A number field. The default is stored with invariant formatting.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The number stored on a new block.</param>
    /// <param name="min">The smallest value. <see langword="null"/> leaves it unset.</param>
    /// <param name="max">The largest value. <see langword="null"/> leaves it unset.</param>
    /// <param name="step">The step. <see langword="null"/> leaves it unset.</param>
    /// <param name="group">The inspector group. The default is Content.</param>
    /// <returns>The field.</returns>
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

    /// <summary>A select field in the Content group.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="label">The label.</param>
    /// <param name="defaultValue">The value stored on a new block.</param>
    /// <param name="options">The choices, as stored value and label pairs.</param>
    /// <returns>The field.</returns>
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