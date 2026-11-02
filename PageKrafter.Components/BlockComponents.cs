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

public abstract class BlockComponentBase : ComponentBase
{
    [Parameter]
    public BlockRenderContext Context { get; set; } = default!;

    [Parameter]
    public IReadOnlyDictionary<string, MarkupString> Slots { get; set; } = new Dictionary<string, MarkupString>();

    protected bool IsEditor => Context.IsEditor;

    protected bool IsEmail => Context.IsEmail;

    protected string Value(string name) => Context.Value(name);

    protected MarkupString Slot(string name = SlotDefinition.Default) => Slots.TryGetValue(name, out var html) ? html : default;

    /// <summary>Spread onto the element that holds a slot: <c>&lt;div @attributes="SlotAttributes()"&gt;@Slot()&lt;/div&gt;</c>.</summary>
    protected IReadOnlyDictionary<string, object> SlotAttributes(string name = SlotDefinition.Default) => Context.SlotAttributeMap(name);

    protected IReadOnlyDictionary<string, object> Editable(string name, bool rich = false) => Context.EditableAttributeMap(name, rich);
}

/// <summary>Renders block components to HTML outside the circuit's renderer, one instance per scope.</summary>
public sealed class BlockComponentRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : IAsyncDisposable
{
    private HtmlRenderer? renderer;

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

    public async ValueTask DisposeAsync()
    {
        if (renderer is not null)
        {
            await renderer.DisposeAsync();
        }
    }
}
