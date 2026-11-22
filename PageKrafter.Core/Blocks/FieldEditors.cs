namespace PageKrafter.Blocks;

/// <summary>
/// Built-in inspector editors. Any other string is a custom editor that the host maps to a Razor
/// component with <c>AddPageKrafterFieldEditor</c>. Mapping a built-in name replaces it, which is how
/// a host swaps the plain image URL box for its own file picker.
/// </summary>
public static class FieldEditors
{
    /// <summary>A single-line text box. The value is <c>text</c>.</summary>
    public const string Text = "text";

    /// <summary>A multi-line text box. The value is <c>textarea</c>.</summary>
    public const string TextArea = "textarea";

    /// <summary>HTML with a button that opens the registered rich text editor. The value is <c>richtext</c>.</summary>
    public const string RichText = "richtext";

    /// <summary>A box for raw HTML. The value is <c>html</c>.</summary>
    public const string Html = "html";

    /// <summary>A number box. The value is <c>number</c>.</summary>
    public const string Number = "number";

    /// <summary>A checkbox. The stored value is <c>true</c> or <c>false</c>. The value is <c>checkbox</c>.</summary>
    public const string Checkbox = "checkbox";

    /// <summary>A drop-down. The value is <c>select</c>.</summary>
    public const string Select = "select";

    /// <summary>A color box. The value is <c>color</c>.</summary>
    public const string Color = "color";

    /// <summary>A URL box. The placeholder is <c>https://</c>. The value is <c>url</c>.</summary>
    public const string Url = "url";

    /// <summary>An image URL box. It browses for images when a file picker is registered. The value is <c>image</c>.</summary>
    public const string Image = "image";
}

/// <summary>Inspector groups, in the order Content, Style, Advanced.</summary>
public static class FieldGroups
{
    /// <summary>The content group. The value is <c>Content</c>.</summary>
    public const string Content = "Content";

    /// <summary>The style group. The value is <c>Style</c>.</summary>
    public const string Style = "Style";

    /// <summary>The advanced group. The value is <c>Advanced</c>.</summary>
    public const string Advanced = "Advanced";
}

/// <summary>Palette groups. Layout, Basic, Media, and Advanced are listed first. Any other category follows them.</summary>
public static class BlockCategories
{
    /// <summary>The layout group. The value is <c>Layout</c>.</summary>
    public const string Layout = "Layout";

    /// <summary>The basic group. The value is <c>Basic</c>.</summary>
    public const string Basic = "Basic";

    /// <summary>The media group. The value is <c>Media</c>.</summary>
    public const string Media = "Media";

    /// <summary>The advanced group. The value is <c>Advanced</c>.</summary>
    public const string Advanced = "Advanced";

    internal static int SortKey(string category) => category switch
    {
        Layout => 0,
        Basic => 1,
        Media => 2,
        Advanced => 3,
        _ => 4,
    };
}