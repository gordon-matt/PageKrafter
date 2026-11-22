namespace PageKrafter.Documents;

/// <summary>Whether a document or a block is for a web page, an email, or both. A block's modes are flags. A document is one of the two.</summary>
[Flags]
public enum DocumentMode
{
    /// <summary>A web page. Blocks render as HTML elements.</summary>
    Web = 1,

    /// <summary>An email. Blocks render as tables with inline styles.</summary>
    Email = 2,

    /// <summary>Both web and email. A document stored as this value is read back as web.</summary>
    All = Web | Email,
}