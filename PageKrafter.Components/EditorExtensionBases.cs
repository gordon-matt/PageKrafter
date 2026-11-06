namespace PageKrafter.Components;

/// <summary>
/// Base for a custom inspector field. Register it with <c>AddPageKrafterFieldEditor&lt;T&gt;("name")</c>
/// and set <see cref="BlockField.Editor"/> to the same name.
/// </summary>
public abstract class FieldEditorBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the field.
    /// </summary>
    [Parameter]
    public BlockField Field { get; set; } = default!;

    /// <summary>
    /// Gets or sets the node.
    /// </summary>
    [Parameter]
    public BlockNode Node { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value changed.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Gets the input identifier.
    /// </summary>
    protected string InputId => $"pk-{Node.Id}-{Field.Name}";

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected Task SetValueAsync(string value) => ValueChanged.InvokeAsync(value);
}

/// <summary>
/// Represents the property change.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="Value">The value.</param>
public sealed record PropertyChange(string Name, string Value);

/// <summary>
/// Base for a block's own inspector, set with <see cref="BlockDefinition.EditorComponent"/>. It replaces
/// the generated content fields, which suits blocks that need a richer editor than a list of fields.
/// </summary>
public abstract class BlockEditorBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the node.
    /// </summary>
    [Parameter]
    public BlockNode Node { get; set; } = default!;

    /// <summary>
    /// Gets or sets the definition.
    /// </summary>
    [Parameter]
    public BlockDefinition Definition { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback raised for the change event.
    /// </summary>
    [Parameter]
    public EventCallback<PropertyChange> OnChange { get; set; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The value.</returns>
    protected string Value(string name) =>
        Node.Get(name) ?? Definition.Fields.FirstOrDefault(field => field.Name == name)?.DefaultValue ?? string.Empty;

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected Task SetAsync(string name, string value) => OnChange.InvokeAsync(new PropertyChange(name, value));
}