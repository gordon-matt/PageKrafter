namespace PageKrafter.Blocks.BuiltIn;

/// <summary>A plain box for grouping blocks, for example a card inside a column.</summary>
public sealed class ContainerBlock : BlockDefinition
{
    private static readonly IReadOnlyList<SlotDefinition> Slots = [new(SlotDefinition.Default, "this box")];

    public override string Type => BlockTypes.Container;

    public override string Label => "Box";

    public override string Category => BlockCategories.Layout;

    public override string Icon => BlockIcons.Container;

    public override string Description => "Groups blocks so they can share a background or border.";

    public override int Order => 2;

    public override DocumentMode Modes => DocumentMode.All;

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Select("tag", "Element", "div", ("div", "div"), ("article", "article"), ("aside", "aside"), ("header", "header"), ("footer", "footer"), ("nav", "nav")) with { Modes = DocumentMode.Web, Group = FieldGroups.Advanced },
        BlockField.Text("maxWidth", "Max width") with { Placeholder = "e.g. 720px", Modes = DocumentMode.Web },
        BlockField.Text("border", "Border", group: FieldGroups.Style) with { Placeholder = "e.g. 1px solid #e5e7eb" },
        BlockField.Text("shadow", "Shadow", group: FieldGroups.Style) with { Placeholder = "e.g. 0 4px 16px rgba(0,0,0,.08)", Modes = DocumentMode.Web },
    ];

    public override IReadOnlyList<SlotDefinition> GetSlots(BlockNode node) => Slots;

    public override IReadOnlyCollection<string> SelfAppliedStyles(DocumentMode mode) =>
        mode == DocumentMode.Email ? EmailMarkup.CellStyles : [];

    public override BlockNode Create() => base.Create().Set(CommonStyles.Padding, "16px");

    public override async ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string children = await context.RenderSlotAsync();
        if (context.IsEmail)
        {
            string cell = EmailMarkup.CellStyle(context) + Css.Declarations(("border", context.Value("border")));
            return $"""<table {EmailMarkup.Table} class="pk-box"><tr><td style="{Html.Encode(cell)}"{context.SlotAttributes()}>{children}</td></tr></table>""";
        }

        string tag = context.Value("tag") is "article" or "aside" or "header" or "footer" or "nav" ? context.Value("tag") : "div";
        string maxWidth = context.Style("maxWidth");
        string style = Css.Declarations(
            ("max-width", maxWidth),
            ("margin-left", maxWidth.Length > 0 ? "auto" : string.Empty),
            ("margin-right", maxWidth.Length > 0 ? "auto" : string.Empty),
            ("border", context.Value("border")),
            ("box-shadow", context.Value("shadow")));

        return $"""<{tag} class="pk-box" style="{Html.Encode(style)}"{context.SlotAttributes()}>{children}</{tag}>""";
    }
}