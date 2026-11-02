namespace PageKrafter.Rendering;

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

    public BlockNode Node { get; }

    public BlockDefinition Definition { get; }

    public PageDocument Document { get; }

    public RenderOptions Options { get; }

    public IServiceProvider Services { get; }

    public IHtmlContentSanitizer Sanitizer { get; }

    public CancellationToken CancellationToken { get; }

    public DocumentMode Mode => Document.Mode;

    public bool IsEmail => Document.Mode == DocumentMode.Email;

    /// <summary>True when rendering for the editor canvas, where placeholders and selection markers are wanted.</summary>
    public bool IsEditor => Options.Purpose == RenderPurpose.Editor;

    /// <summary>The stored prop, or the field default when the prop has never been set.</summary>
    public string Value(string name) =>
        Node.Get(name)
        ?? Definition.Fields.FirstOrDefault(field => string.Equals(field.Name, name, StringComparison.Ordinal))?.DefaultValue
        ?? string.Empty;

    public string Text(string name) => Html.Encode(Value(name));

    public string RichText(string name) => Sanitizer.Sanitize(Value(name));

    public string Url(string name, bool allowDataImage = false) => Html.Encode(Html.SafeUrl(Value(name), allowDataImage));

    public string Style(string name) => Css.Clean(Value(name));

    public bool Bool(string name) => string.Equals(Value(name), "true", StringComparison.OrdinalIgnoreCase);

    public int Int(string name, int fallback = 0) =>
        decimal.TryParse(Value(name), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value) ? (int)Math.Round(value) : fallback;

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

    public IReadOnlyDictionary<string, object> EditableAttributeMap(string name, bool rich = false) =>
        IsEditor
            ? new Dictionary<string, object>(StringComparer.Ordinal) { ["data-pk-edit"] = name, ["data-pk-edit-kind"] = rich ? "rich" : "plain" }
            : EmptyMap;

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
