namespace PageKrafter.Rendering;

/// <summary>What a block can use while it renders: the node, the document, and helpers that encode, sanitize, and render children.</summary>
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

    /// <summary>The block being rendered.</summary>
    public BlockNode Node { get; }

    /// <summary>The definition for <see cref="Node"/>.</summary>
    public BlockDefinition Definition { get; }

    /// <summary>The document that contains the block.</summary>
    public PageDocument Document { get; }

    /// <summary>Export or editor options for this render.</summary>
    public RenderOptions Options { get; }

    /// <summary>The current scope. A block can resolve a service the host registered.</summary>
    public IServiceProvider Services { get; }

    /// <summary>The sanitizer used by <see cref="RichText"/>.</summary>
    public IHtmlContentSanitizer Sanitizer { get; }

    /// <summary>Cancels this render. A block should pass it to work it awaits.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>The document mode.</summary>
    public DocumentMode Mode => Document.Mode;

    /// <summary><see langword="true"/> only when the document is an email.</summary>
    public bool IsEmail => Document.Mode == DocumentMode.Email;

    /// <summary>True when rendering for the editor canvas, where placeholders and selection markers are wanted.</summary>
    public bool IsEditor => Options.Purpose == RenderPurpose.Editor;

    /// <summary>The stored prop, or the field default when the prop has never been set, or an empty string when the field has no default.</summary>
    /// <param name="name">The prop name.</param>
    /// <returns>The text.</returns>
    public string Value(string name) =>
        Node.Get(name)
        ?? Definition.Fields.FirstOrDefault(field => string.Equals(field.Name, name, StringComparison.Ordinal))?.DefaultValue
        ?? string.Empty;

    /// <summary><see cref="Value"/> encoded for element content or a double-quoted attribute.</summary>
    /// <param name="name">The prop name.</param>
    /// <returns>The encoded text.</returns>
    public string Text(string name) => Html.Encode(Value(name));

    /// <summary><see cref="Value"/> passed through <see cref="Sanitizer"/>.</summary>
    /// <param name="name">The prop name.</param>
    /// <returns>The sanitized HTML.</returns>
    public string RichText(string name) => Sanitizer.Sanitize(Value(name));

    /// <summary><see cref="Value"/> kept only when <see cref="Html.SafeUrl"/> accepts it, then encoded.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="allowDataImage"><see langword="true"/> also allows a <c>data:image</c> URL that is not SVG.</param>
    /// <returns>The encoded URL, or an empty string.</returns>
    public string Url(string name, bool allowDataImage = false) => Html.Encode(Html.SafeUrl(Value(name), allowDataImage));

    /// <summary><see cref="Value"/> passed through <see cref="Css.Clean"/>.</summary>
    /// <param name="name">The prop name.</param>
    /// <returns>The cleaned declaration value.</returns>
    public string Style(string name) => Css.Clean(Value(name));

    /// <summary>Whether the prop is the text <c>true</c>, ignoring case.</summary>
    /// <param name="name">The prop name.</param>
    /// <returns><see langword="true"/> when the value is <c>true</c>.</returns>
    public bool Bool(string name) => string.Equals(Value(name), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>The prop parsed as an invariant decimal and rounded. Anything that does not parse returns <paramref name="fallback"/>.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="fallback">The value used when the prop is missing or not a number. The default is 0.</param>
    /// <returns>The rounded number, or <paramref name="fallback"/>.</returns>
    public int Int(string name, int fallback = 0) =>
        decimal.TryParse(Value(name), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value) ? (int)Math.Round(value) : fallback;

    /// <summary>Renders the children of a slot, in order, and joins the HTML. An empty slot returns an empty string.</summary>
    /// <param name="slot">The slot name. The default is <see cref="SlotDefinition.Default"/>.</param>
    /// <returns>The children's HTML.</returns>
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
    /// Editor attributes for the element that holds a slot. Empty when this is not an editor render.
    /// The map has <c>data-pk-slot</c>, <c>data-pk-owner</c>, and <c>data-pk-slot-label</c>. An empty slot also has <c>data-pk-empty</c>.
    /// </summary>
    /// <param name="slot">The slot name. The default is <see cref="SlotDefinition.Default"/>.</param>
    /// <returns>The attributes, or an empty map.</returns>
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
    /// Editor attributes for an element that can be edited in place. Empty when this is not an editor render.
    /// The map has <c>data-pk-edit</c> and <c>data-pk-edit-kind</c> (<c>rich</c> or <c>plain</c>). A field that accepts tokens also has <c>data-pk-token</c>.
    /// </summary>
    /// <param name="name">The prop name.</param>
    /// <param name="rich"><see langword="true"/> marks the field as rich text.</param>
    /// <returns>The attributes, or an empty map.</returns>
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