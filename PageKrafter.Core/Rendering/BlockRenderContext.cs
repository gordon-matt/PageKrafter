namespace PageKrafter.Rendering;

/// <summary>
/// Represents the block render context.
/// </summary>
public sealed class BlockRenderContext
{
    private readonly PageRenderer renderer;
    private readonly int depth;

    internal BlockRenderContext(
        PageRenderer renderer,
        BlockNode node,
        BlockDefinition definition,
        PageDocument document,
        RenderOptions options,
        int depth,
        IServiceProvider services,
        IHtmlContentSanitizer sanitizer,
        CancellationToken cancellationToken)
    {
        this.renderer = renderer;
        this.depth = depth;
        Node = node;
        Definition = definition;
        Document = document;
        Options = options;
        Services = services;
        Sanitizer = sanitizer;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the node.
    /// </summary>
    public BlockNode Node { get; }

    /// <summary>
    /// Gets the definition.
    /// </summary>
    public BlockDefinition Definition { get; }

    /// <summary>
    /// Gets the document.
    /// </summary>
    public PageDocument Document { get; }

    /// <summary>
    /// Gets the options.
    /// </summary>
    public RenderOptions Options { get; }

    /// <summary>
    /// Gets the services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets the sanitizer.
    /// </summary>
    public IHtmlContentSanitizer Sanitizer { get; }

    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the mode.
    /// </summary>
    public DocumentMode Mode => Document.Mode;

    /// <summary>
    /// Gets a value indicating whether this instance is email.
    /// </summary>
    public bool IsEmail => Document.Mode == DocumentMode.Email;

    /// <summary>True when rendering for the editor canvas, where placeholders and selection markers are wanted.</summary>
    public bool IsEditor => Options.Purpose == RenderPurpose.Editor;

    /// <summary>The stored prop, or the field default when the prop has never been set.</summary>
    public string Value(string name) =>
        Node.Get(name)
        ?? Definition.Fields.FirstOrDefault(field => string.Equals(field.Name, name, StringComparison.Ordinal))?.DefaultValue
        ?? string.Empty;

    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The text.</returns>
    public string Text(string name) => Html.Encode(Value(name));

    /// <summary>
    /// Gets the rich text.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The rich text.</returns>
    public string RichText(string name) => Sanitizer.Sanitize(Value(name));

    /// <summary>
    /// Gets the URL.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="allowDataImage">The allow data image.</param>
    /// <returns>The URL.</returns>
    public string Url(string name, bool allowDataImage = false) => Html.Encode(Html.SafeUrl(Value(name), allowDataImage));

    /// <summary>
    /// Gets the style.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The style.</returns>
    public string Style(string name) => Css.Clean(Value(name));

    /// <summary>
    /// Gets the bool.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>true when the condition is met; otherwise, false.</returns>
    public bool Bool(string name) => string.Equals(Value(name), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the int.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="fallback">The fallback.</param>
    /// <returns>The int.</returns>
    public int Int(string name, int fallback = 0) =>
        decimal.TryParse(Value(name), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value) ? (int)Math.Round(value) : fallback;

    /// <summary>
    /// Renders the slot.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the string.</returns>
    public async Task<string> RenderSlotAsync(string slot = SlotDefinition.Default)
    {
        var children = Node.ChildrenOf(slot);
        if (children.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var child in children)
        {
            builder.Append(await renderer.RenderNodeAsync(child, Document, Options, depth + 1, CancellationToken));
        }

        return builder.ToString();
    }

    /// <summary>Editor markers for the element that holds a slot's children. Empty in exported HTML.</summary>
    public string SlotAttributes(string slot = SlotDefinition.Default) => ToAttributeString(SlotAttributeMap(slot));

    /// <summary>
    /// Gets the slot attribute map.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <returns>The slot attribute map.</returns>
    public IReadOnlyDictionary<string, object> SlotAttributeMap(string slot = SlotDefinition.Default)
    {
        if (!IsEditor)
        {
            return EmptyMap;
        }

        string label = Definition.GetSlots(Node).FirstOrDefault(candidate => candidate.Name == slot)?.Label ?? slot;
        var map = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["data-pk-slot"] = slot,
            ["data-pk-owner"] = Node.Id,
            ["data-pk-slot-label"] = $"Drop blocks into {label}",
        };

        if (Node.ChildrenOf(slot).Count == 0)
        {
            map["data-pk-empty"] = string.Empty;
        }

        return map;
    }

    /// <summary>Marks an element as editable in place by double-clicking it in the canvas. Empty in exported HTML.</summary>
    public string EditableAttributes(string name, bool rich = false) => ToAttributeString(EditableAttributeMap(name, rich));

    /// <summary>
    /// Gets the editable attribute map.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="rich">The rich.</param>
    /// <returns>The editable attribute map.</returns>
    public IReadOnlyDictionary<string, object> EditableAttributeMap(string name, bool rich = false)
    {
        if (!IsEditor)
        {
            return EmptyMap;
        }

        var map = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["data-pk-edit"] = name,
            ["data-pk-edit-kind"] = rich ? "rich" : "plain",
        };

        // The canvas uses this to allow token insertion only in fields that opted in.
        if (Definition.Fields.Any(field => field.AcceptsTokens && string.Equals(field.Name, name, StringComparison.Ordinal)))
        {
            map["data-pk-token"] = string.Empty;
        }

        return map;
    }

    private static readonly IReadOnlyDictionary<string, object> EmptyMap = new Dictionary<string, object>();

    private static string ToAttributeString(IReadOnlyDictionary<string, object> attributes)
    {
        if (attributes.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var (name, value) in attributes)
        {
            builder.Append(' ').Append(name).Append("=\"").Append(Html.Encode(value.ToString())).Append('"');
        }

        return builder.ToString();
    }
}