namespace PageKrafter.Blocks.BuiltIn;

/// <summary>The root of every document. Selecting it in the editor shows the page settings.</summary>
public sealed class PageBlock : BlockDefinition
{
    private static readonly IReadOnlyList<SlotDefinition> Slots = [new(SlotDefinition.Default, "the page")];

    public override string Type => BlockTypes.Page;

    public override string Label => "Page";

    public override string Category => BlockCategories.Layout;

    public override string Icon => BlockIcons.Page;

    public override bool ShowInPalette => false;

    public override DocumentMode Modes => DocumentMode.All;

    public override bool UsesCommonStyles => false;

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Color("background", "Page background", group: FieldGroups.Content),
        BlockField.Color("textColor", "Text color", group: FieldGroups.Content),
        BlockField.Text("fontFamily", "Font family") with { Placeholder = "Inherit from the site" },
        BlockField.Text("maxWidth", "Max width") with { Placeholder = "e.g. 1200px", Modes = DocumentMode.Web },
        BlockField.Text("padding", "Padding") with { Placeholder = "e.g. 0 16px" },
        BlockField.Color("bodyBackground", "Content background", "#ffffff", FieldGroups.Content) with { Modes = DocumentMode.Email },
        BlockField.Number("contentWidth", "Content width (px)", 600, 320, 800, 10) with { Modes = DocumentMode.Email, HelpText = "600px suits most email clients." },
    ];

    public override IReadOnlyList<SlotDefinition> GetSlots(BlockNode node) => Slots;

    public override async ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string children = await context.RenderSlotAsync();
        if (context.IsEmail)
        {
            int width = Math.Clamp(context.Int("contentWidth", 600), 320, 800);
            string outer = Css.Declarations(
                ("background-color", context.Value("background") is { Length: > 0 } background ? background : "#f4f5f7"),
                ("color", context.Value("textColor") is { Length: > 0 } color ? color : "#1f2937"),
                ("font-family", context.Value("fontFamily") is { Length: > 0 } font ? font : "Arial, Helvetica, sans-serif"),
                ("padding", context.Value("padding") is { Length: > 0 } padding ? padding : "24px 0"),
                ("margin", "0"));

            string inner = Css.Declarations(
                ("width", "100%"),
                ("max-width", $"{width}px"),
                ("margin", "0 auto"),
                ("background-color", context.Value("bodyBackground")));

            return $"""<div class="pk-page pk-email" style="{Html.Encode(outer)}"><table {EmailMarkup.Table} align="center" class="pk-email-body" width="{width}" style="{Html.Encode(inner)}"><tr><td{context.SlotAttributes()}>{children}</td></tr></table></div>""";
        }

        string maxWidth = context.Style("maxWidth");
        string style = Css.Declarations(
            ("background-color", context.Value("background")),
            ("color", context.Value("textColor")),
            ("font-family", context.Value("fontFamily")),
            ("padding", context.Value("padding")),
            ("max-width", maxWidth),
            ("margin", maxWidth.Length > 0 ? "0 auto" : string.Empty));

        string styleAttribute = style.Length > 0 ? $" style=\"{Html.Encode(style)}\"" : string.Empty;
        return $"""<div class="pk-page"{styleAttribute}{context.SlotAttributes()}>{children}</div>""";
    }
}
