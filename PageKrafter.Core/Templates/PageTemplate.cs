namespace PageKrafter.Templates;

/// <summary>Whether applying a template replaces the document or inserts one block.</summary>
public enum TemplateKind
{
    /// <summary>A whole page or email. Applying it replaces the current document.</summary>
    Page,

    /// <summary>One block and its children, such as a two-column row. It is dragged into the page like a block.</summary>
    Section,
}

/// <summary>A reusable page or section. Saved templates and built-in templates use the same shape.</summary>
public sealed record PageTemplate
{
    /// <summary>The template id. A new saved template starts with <c>t</c> and then the usual block-id characters.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The name shown in the template panel.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The text shown beside the name. Empty shows the name only.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>The panel group. A blank category is saved as "My templates".</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Whether this replaces the document or inserts one block.</summary>
    public TemplateKind Kind { get; init; }

    /// <summary>The document mode this template is for. It is listed only for a document in the same mode.</summary>
    public DocumentMode Mode { get; init; } = DocumentMode.Web;

    /// <summary>For a page template this is the page root; for a section template it is the block to insert.</summary>
    public BlockNode Content { get; init; } = new();

    /// <summary>True for templates from an <see cref="ITemplateProvider"/>, which editors can use but not change or delete.</summary>
    public bool IsReadOnly { get; init; }

    /// <summary>When the template was last saved, in UTC. <see langword="null"/> when the source does not record it.</summary>
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

    /// <summary>A new document in this template's mode, with a fresh copy of the content.</summary>
    /// <returns>The document.</returns>
    public PageDocument CreateDocument() => new() { Mode = Mode, Root = Instantiate() };
}

/// <summary>The name and kind an editor submits when saving a template. The block tree is passed separately.</summary>
public sealed record TemplateForm
{
    /// <summary>The template to replace. <see langword="null"/> or blank creates one.</summary>
    public string? Id { get; init; }

    /// <summary>The name. It is trimmed, and it cannot be empty or longer than <see cref="TemplateService.MaxNameLength"/>.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The description. It is trimmed.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>The panel group. Blank becomes "My templates".</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Page or section. A page template must be a whole page, and a section template must not be.</summary>
    public TemplateKind Kind { get; init; }

    /// <summary>The document mode stored on the template.</summary>
    public DocumentMode Mode { get; init; } = DocumentMode.Web;
}