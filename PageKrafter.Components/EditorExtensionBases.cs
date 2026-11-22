namespace PageKrafter.Components;

/// <summary>
/// Base for a custom inspector field. Register it with <c>AddPageKrafterFieldEditor&lt;T&gt;("name")</c>
/// and set <see cref="BlockField.Editor"/> to the same name.
/// </summary>
public abstract class FieldEditorBase : ComponentBase
{
    /// <summary>The field this editor is drawing.</summary>
    [Parameter]
    public BlockField Field { get; set; } = default!;

    /// <summary>The block that owns the field.</summary>
    [Parameter]
    public BlockNode Node { get; set; } = default!;

    /// <summary>The current prop text, or the field default when the prop has never been set.</summary>
    [Parameter]
    public string Value { get; set; } = string.Empty;

    /// <summary>Raised with the new prop text. Call <see cref="SetValueAsync"/> rather than invoking this directly.</summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>An id for the input and its label: <c>pk-</c>, the block id, and the field name.</summary>
    protected string InputId => $"pk-{Node.Id}-{Field.Name}";

    /// <summary>Writes the prop and records the edit.</summary>
    /// <param name="value">The text to store.</param>
    /// <returns>A task that completes when the editor has accepted the value.</returns>
    protected Task SetValueAsync(string value) => ValueChanged.InvokeAsync(value);
}

/// <summary>One prop written by a custom block inspector.</summary>
/// <param name="Name">The prop name.</param>
/// <param name="Value">The text to store.</param>
public sealed record PropertyChange(string Name, string Value);

/// <summary>
/// Base for a block's own inspector, set with <see cref="BlockDefinition.EditorComponent"/>. It replaces
/// the generated content fields, which suits blocks that need a richer editor than a list of fields.
/// </summary>
public abstract class BlockEditorBase : ComponentBase
{
    /// <summary>The block being edited.</summary>
    [Parameter]
    public BlockNode Node { get; set; } = default!;

    /// <summary>The block's definition, including its fields and defaults.</summary>
    [Parameter]
    public BlockDefinition Definition { get; set; } = default!;

    /// <summary>Raised with the prop that changed. Call <see cref="SetAsync"/> rather than invoking this directly.</summary>
    [Parameter]
    public EventCallback<PropertyChange> OnChange { get; set; }

    /// <summary>The stored prop, the field default, or an empty string.</summary>
    /// <param name="name">The prop name.</param>
    /// <returns>The text.</returns>
    protected string Value(string name) =>
        Node.Get(name) ?? Definition.Fields.FirstOrDefault(field => field.Name == name)?.DefaultValue ?? string.Empty;

    /// <summary>Writes one prop and records the edit.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="value">The text to store.</param>
    /// <returns>A task that completes when the editor has accepted the value.</returns>
    protected Task SetAsync(string name, string value) => OnChange.InvokeAsync(new PropertyChange(name, value));
}