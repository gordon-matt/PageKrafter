namespace PageKrafter.Blocks;

/// <summary>Type keys of the built-in blocks. A host block should use its own prefix so these names stay free.</summary>
public static class BlockTypes
{
    /// <summary>The page root. The value is <c>page</c>.</summary>
    public const string Page = "page";

    /// <summary>A full-width band. The value is <c>section</c>.</summary>
    public const string Section = "section";

    /// <summary>Side-by-side columns. The value is <c>columns</c>.</summary>
    public const string Columns = "columns";

    /// <summary>A grouping box. The value is <c>container</c>.</summary>
    public const string Container = "container";

    /// <summary>A heading. The value is <c>heading</c>.</summary>
    public const string Heading = "heading";

    /// <summary>Rich text. The value is <c>text</c>.</summary>
    public const string Text = "text";

    /// <summary>An image. The value is <c>image</c>.</summary>
    public const string Image = "image";

    /// <summary>A link styled as a button. The value is <c>button</c>.</summary>
    public const string Button = "button";

    /// <summary>A horizontal line. The value is <c>divider</c>.</summary>
    public const string Divider = "divider";

    /// <summary>Empty vertical space. The value is <c>spacer</c>.</summary>
    public const string Spacer = "spacer";

    /// <summary>A video. The value is <c>video</c>. It is web only.</summary>
    public const string Video = "video";

    /// <summary>Sanitized raw markup. The value is <c>html</c>.</summary>
    public const string Html = "html";
}