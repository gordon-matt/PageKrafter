namespace PageKrafter.Blocks;

/// <summary>
/// Built-in inspector editors. Any other string is a custom editor that the host maps to a Razor
/// component with <c>AddPageKrafterFieldEditor</c>. Mapping a built-in name replaces it, which is how
/// a host swaps the plain image URL box for its own file picker.
/// </summary>
public static class FieldEditors
{
    /// <summary>
    /// The text constant.
    /// </summary>
    public const string Text = "text";
    /// <summary>
    /// The text area constant.
    /// </summary>
    public const string TextArea = "textarea";
    /// <summary>
    /// The rich text constant.
    /// </summary>
    public const string RichText = "richtext";
    /// <summary>
    /// The HTML constant.
    /// </summary>
    public const string Html = "html";
    /// <summary>
    /// The number constant.
    /// </summary>
    public const string Number = "number";
    /// <summary>
    /// The checkbox constant.
    /// </summary>
    public const string Checkbox = "checkbox";
    /// <summary>
    /// The select constant.
    /// </summary>
    public const string Select = "select";
    /// <summary>
    /// The color constant.
    /// </summary>
    public const string Color = "color";
    /// <summary>
    /// The URL constant.
    /// </summary>
    public const string Url = "url";
    /// <summary>
    /// The image constant.
    /// </summary>
    public const string Image = "image";
}

/// <summary>
/// Provides field groups.
/// </summary>
public static class FieldGroups
{
    /// <summary>
    /// The content constant.
    /// </summary>
    public const string Content = "Content";
    /// <summary>
    /// The style constant.
    /// </summary>
    public const string Style = "Style";
    /// <summary>
    /// The advanced constant.
    /// </summary>
    public const string Advanced = "Advanced";
}

/// <summary>
/// Provides block categories.
/// </summary>
public static class BlockCategories
{
    /// <summary>
    /// The layout constant.
    /// </summary>
    public const string Layout = "Layout";
    /// <summary>
    /// The basic constant.
    /// </summary>
    public const string Basic = "Basic";
    /// <summary>
    /// The media constant.
    /// </summary>
    public const string Media = "Media";
    /// <summary>
    /// The advanced constant.
    /// </summary>
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