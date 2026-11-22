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

/// <summary>A Razor component that renders one block. The renderer sets <see cref="Context"/> and <see cref="Slots"/> before the component runs.</summary>
public abstract class BlockComponentBase : ComponentBase
{
    /// <summary>The block, the document, and the encoding helpers.</summary>
    [Parameter]
    public BlockRenderContext Context { get; set; } = default!;

    /// <summary>Each slot's children, already rendered. The markup is trusted because the renderer produced it.</summary>
    [Parameter]
    public IReadOnlyDictionary<string, MarkupString> Slots { get; set; } = new Dictionary<string, MarkupString>();

    /// <summary><see langword="true"/> when the HTML is for the canvas.</summary>
    protected bool IsEditor => Context.IsEditor;

    /// <summary><see langword="true"/> only when the document is an email.</summary>
    protected bool IsEmail => Context.IsEmail;

    /// <summary>The stored prop, the field default, or an empty string.</summary>
    /// <param name="name">The prop name.</param>
    /// <returns>The text.</returns>
    protected string Value(string name) => Context.Value(name);

    /// <summary>The rendered children of a slot. A missing slot is empty markup.</summary>
    /// <param name="name">The slot name. The default is the block's main slot.</param>
    /// <returns>The children's HTML.</returns>
    protected MarkupString Slot(string name = SlotDefinition.Default) => Slots.TryGetValue(name, out var html) ? html : default;

    /// <summary>Spread onto the element that holds a slot: <c>&lt;div @attributes="SlotAttributes()"&gt;@Slot()&lt;/div&gt;</c>.</summary>
    protected IReadOnlyDictionary<string, object> SlotAttributes(string name = SlotDefinition.Default) => Context.SlotAttributeMap(name);

    /// <summary>Attributes that let the canvas edit this element in place. Empty in exported HTML.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="rich"><see langword="true"/> marks the field as rich text.</param>
    /// <returns>The attributes to spread onto the element.</returns>
    protected IReadOnlyDictionary<string, object> Editable(string name, bool rich = false) => Context.EditableAttributeMap(name, rich);
}

/// <summary>Renders block components to HTML outside the circuit's renderer, one instance per scope.</summary>
public sealed class BlockComponentRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : IAsyncDisposable
{
    private HtmlRenderer? renderer;

    /// <summary>Renders one block component to an HTML string. The same renderer is reused for later blocks in the scope.</summary>
    /// <param name="componentType">The component to render.</param>
    /// <param name="parameters">The parameter values, including the context and the slots.</param>
    /// <returns>The component's HTML.</returns>
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