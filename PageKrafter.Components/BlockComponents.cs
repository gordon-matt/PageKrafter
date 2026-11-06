using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace PageKrafter.Components;

/// <summary>
/// A block whose HTML comes from a Razor component, for hosts that would rather write markup than
/// C# strings. Child slots are rendered first and passed to the component as <see cref="BlockComponentBase.Slots"/>.
/// </summary>
public abstract class ComponentBlockDefinition<TComponent> : BlockDefinition
    where TComponent : BlockComponentBase
{
    /// <inheritdoc/>
    public override async ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        var slots = new Dictionary<string, MarkupString>(StringComparer.Ordinal);
        foreach (var slot in GetSlots(context.Node))
        {
            slots[slot.Name] = new MarkupString(await context.RenderSlotAsync(slot.Name));
        }

        var renderer = context.Services.GetRequiredService<BlockComponentRenderer>();
        return await renderer.RenderAsync(typeof(TComponent), new Dictionary<string, object?>
        {
            [nameof(BlockComponentBase.Context)] = context,
            [nameof(BlockComponentBase.Slots)] = slots,
        });
    }
}

/// <summary>
/// Represents the block component base.
/// </summary>
public abstract class BlockComponentBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the context.
    /// </summary>
    [Parameter]
    public BlockRenderContext Context { get; set; } = default!;

    /// <summary>
    /// Gets or sets the slots.
    /// </summary>
    [Parameter]
    public IReadOnlyDictionary<string, MarkupString> Slots { get; set; } = new Dictionary<string, MarkupString>();

    /// <summary>
    /// Gets a value indicating whether this instance is editor.
    /// </summary>
    protected bool IsEditor => Context.IsEditor;

    /// <summary>
    /// Gets a value indicating whether this instance is email.
    /// </summary>
    protected bool IsEmail => Context.IsEmail;

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The value.</returns>
    protected string Value(string name) => Context.Value(name);

    /// <summary>
    /// Gets the slot.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The slot.</returns>
    protected MarkupString Slot(string name = SlotDefinition.Default) => Slots.TryGetValue(name, out var html) ? html : default;

    /// <summary>Spread onto the element that holds a slot: <c>&lt;div @attributes="SlotAttributes()"&gt;@Slot()&lt;/div&gt;</c>.</summary>
    protected IReadOnlyDictionary<string, object> SlotAttributes(string name = SlotDefinition.Default) => Context.SlotAttributeMap(name);

    /// <summary>
    /// Gets the editable.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="rich">The rich.</param>
    /// <returns>The editable.</returns>
    protected IReadOnlyDictionary<string, object> Editable(string name, bool rich = false) => Context.EditableAttributeMap(name, rich);
}

/// <summary>Renders block components to HTML outside the circuit's renderer, one instance per scope.</summary>
public sealed class BlockComponentRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : IAsyncDisposable
{
    private HtmlRenderer? renderer;

    /// <summary>
    /// Renders the value.
    /// </summary>
    /// <param name="componentType">The component type.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the string.</returns>
    public async Task<string> RenderAsync(Type componentType, IDictionary<string, object?> parameters)
    {
        renderer ??= new HtmlRenderer(services, loggerFactory);
        var current = renderer;
        return await current.Dispatcher.InvokeAsync(async () =>
        {
            var output = await current.RenderComponentAsync(componentType, ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });
    }

    /// <summary>
    /// Releases the resources used by this instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (renderer is not null)
        {
            await renderer.DisposeAsync();
        }
    }
}