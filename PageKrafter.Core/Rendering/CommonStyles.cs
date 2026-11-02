namespace PageKrafter.Rendering;

/// <summary>
/// Style fields shared by every block that opts in. The renderer applies them to the block's root
/// element, so host blocks get spacing, colours and visibility without writing any code.
/// </summary>
public static class CommonStyles
{
    public const string Padding = "_padding";
    public const string Margin = "_margin";
    public const string Background = "_background";
    public const string Color = "_color";
    public const string Align = "_align";
    public const string Radius = "_radius";
    public const string CssClass = "_class";
    public const string Anchor = "_anchor";
    public const string Hide = "_hide";

    public static IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Text(Padding, "Padding", group: FieldGroups.Style) with { Placeholder = "e.g. 16px or 16px 24px" },
        BlockField.Text(Margin, "Margin", group: FieldGroups.Style) with { Placeholder = "e.g. 0 0 24px" },
        BlockField.Color(Background, "Background"),
        BlockField.Color(Color, "Text color"),
        BlockField.Select(Align, "Alignment", string.Empty, (string.Empty, "Default"), ("left", "Left"), ("center", "Center"), ("right", "Right"), ("justify", "Justify")) with { Group = FieldGroups.Style },
        BlockField.Text(Radius, "Corner radius", group: FieldGroups.Style) with { Placeholder = "e.g. 8px" },
        BlockField.Text(CssClass, "CSS classes", group: FieldGroups.Advanced) with { HelpText = "Classes from your site's stylesheet." },
        BlockField.Text(Anchor, "Anchor ID", group: FieldGroups.Advanced) with { HelpText = "Lets links jump to this block with #id." },
        BlockField.Select(Hide, "Visibility", string.Empty, (string.Empty, "Always visible"), ("mobile", "Hide on mobile"), ("desktop", "Hide on desktop")) with { Group = FieldGroups.Advanced, Modes = DocumentMode.Web },
    ];

    internal static ResolvedStyles Resolve(BlockNode node, DocumentMode mode, IReadOnlyCollection<string> skip)
    {
        string Read(string name) => skip.Contains(name) ? string.Empty : node.Get(name) ?? string.Empty;

        string style = Css.Declarations(
            ("padding", Read(Padding)),
            ("margin", Read(Margin)),
            ("background-color", Read(Background)),
            ("color", Read(Color)),
            ("text-align", Read(Align)),
            ("border-radius", Read(Radius)));

        string cssClass = Css.ClassNames(Read(CssClass));
        if (mode == DocumentMode.Web)
        {
            string hide = Read(Hide) switch
            {
                "mobile" => "pk-hide-mobile",
                "desktop" => "pk-hide-desktop",
                _ => string.Empty,
            };

            cssClass = string.Join(' ', new[] { cssClass, hide }.Where(part => part.Length > 0));
        }

        return new ResolvedStyles(cssClass, style, Css.Identifier(Read(Anchor)));
    }

    internal readonly record struct ResolvedStyles(string CssClass, string Style, string Id);
}
