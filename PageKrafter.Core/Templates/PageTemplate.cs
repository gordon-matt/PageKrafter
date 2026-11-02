namespace PageKrafter.Templates;

public enum TemplateKind
{
    /// <summary>A whole page or email. Applying it replaces the current document.</summary>
    Page,

    /// <summary>One block and its children, such as a two-column row. It is dragged into the page like a block.</summary>
    Section,
}

public sealed record PageTemplate
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public TemplateKind Kind { get; init; }

    public DocumentMode Mode { get; init; } = DocumentMode.Web;

    /// <summary>For a page template this is the page root; for a section template it is the block to insert.</summary>
    public BlockNode Content { get; init; } = new();

    /// <summary>True for templates from an <see cref="ITemplateProvider"/>, which editors can use but not change or delete.</summary>
    public bool IsReadOnly { get; init; }

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

    public PageDocument CreateDocument() => new() { Mode = Mode, Root = Instantiate() };
}

public sealed record TemplateForm
{
    public string? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public TemplateKind Kind { get; init; }

    public DocumentMode Mode { get; init; } = DocumentMode.Web;
}
