namespace PageKrafter.Templates;

/// <summary>
/// Specifies the template kind.
/// </summary>
public enum TemplateKind
{
    /// <summary>A whole page or email. Applying it replaces the current document.</summary>
    Page,

    /// <summary>One block and its children, such as a two-column row. It is dragged into the page like a block.</summary>
    Section,
}

/// <summary>
/// Represents the page template.
/// </summary>
public sealed record PageTemplate
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    public TemplateKind Kind { get; init; }

    /// <summary>
    /// Gets or sets the mode.
    /// </summary>
    public DocumentMode Mode { get; init; } = DocumentMode.Web;

    /// <summary>For a page template this is the page root; for a section template it is the block to insert.</summary>
    public BlockNode Content { get; init; } = new();

    /// <summary>True for templates from an <see cref="ITemplateProvider"/>, which editors can use but not change or delete.</summary>
    public bool IsReadOnly { get; init; }

    /// <summary>
    /// Gets or sets the updated utc.
    /// </summary>
    public DateTime? UpdatedUtc { get; init; }

    /// <summary>A copy with fresh ids, so the same template can be inserted many times.</summary>
    public BlockNode Instantiate()
    {
        var node = Content.Clone(newIds: true);
        if (Kind == TemplateKind.Page)
        {
            node.Id = PageDocument.RootId;
            node.Type = BlockTypes.Page;
        }

        return node;
    }

    /// <summary>
    /// Creates the document.
    /// </summary>
    /// <returns>The document.</returns>
    public PageDocument CreateDocument() => new() { Mode = Mode, Root = Instantiate() };
}

/// <summary>
/// Represents the template form.
/// </summary>
public sealed record TemplateForm
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    public TemplateKind Kind { get; init; }

    /// <summary>
    /// Gets or sets the mode.
    /// </summary>
    public DocumentMode Mode { get; init; } = DocumentMode.Web;
}