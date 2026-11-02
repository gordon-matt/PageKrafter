namespace PageKrafter.Components;

/// <summary>
/// Base for a custom inspector field. Register it with <c>AddPageKrafterFieldEditor&lt;T&gt;("name")</c>
/// and set <see cref="BlockField.Editor"/> to the same name.
/// </summary>
public abstract class FieldEditorBase : ComponentBase
{
    [Parameter]
    public BlockField Field { get; set; } = default!;

    [Parameter]
    public BlockNode Node { get; set; } = default!;

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    protected string InputId => $"pk-{Node.Id}-{Field.Name}";

    protected Task SetValueAsync(string value) => ValueChanged.InvokeAsync(value);
}

public sealed record PropertyChange(string Name, string Value);

/// <summary>
/// Base for a block's own inspector, set with <see cref="BlockDefinition.EditorComponent"/>. It replaces
/// the generated content fields, which suits blocks that need a richer editor than a list of fields.
/// </summary>
public abstract class BlockEditorBase : ComponentBase
{
    [Parameter]
    public BlockNode Node { get; set; } = default!;

    [Parameter]
    public BlockDefinition Definition { get; set; } = default!;

    [Parameter]
    public EventCallback<PropertyChange> OnChange { get; set; }

    protected string Value(string name) =>
        Node.Get(name) ?? Definition.Fields.FirstOrDefault(field => field.Name == name)?.DefaultValue ?? string.Empty;

    protected Task SetAsync(string name, string value) => OnChange.InvokeAsync(new PropertyChange(name, value));
}
