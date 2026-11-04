namespace PageKrafter.Blocks;

/// <summary>
/// Built-in inspector editors. Any other string is a custom editor that the host maps to a Razor
/// component with <c>AddPageKrafterFieldEditor</c>. Mapping a built-in name replaces it, which is how
/// a host swaps the plain image URL box for its own file picker.
/// </summary>
public static class FieldEditors
{
    public const string Text = "text";
    public const string TextArea = "textarea";
    public const string RichText = "richtext";
    public const string Html = "html";
    public const string Number = "number";
    public const string Checkbox = "checkbox";
    public const string Select = "select";
    public const string Color = "color";
    public const string Url = "url";
    public const string Image = "image";
}

public static class FieldGroups
{
    public const string Content = "Content";
    public const string Style = "Style";
    public const string Advanced = "Advanced";
}

public static class BlockCategories
{
    public const string Layout = "Layout";
    public const string Basic = "Basic";
    public const string Media = "Media";
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