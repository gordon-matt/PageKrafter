namespace PageKrafter.Blocks.BuiltIn;

/// <summary>A full-width band with a centred content area. Most pages are a stack of these.</summary>
public sealed class SectionBlock : BlockDefinition
{
    private static readonly IReadOnlyList<SlotDefinition> Slots = [new(SlotDefinition.Default, "this section")];

    public override string Type => BlockTypes.Section;

    public override string Label => "Section";

    public override string Category => BlockCategories.Layout;

    public override string Icon => BlockIcons.Section;

    public override string Description => "A full-width band that holds other blocks.";

    public override int Order => 0;

    public override DocumentMode Modes => DocumentMode.All;

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Text("maxWidth", "Content width", "1140px") with { Modes = DocumentMode.Web, HelpText = "Width of the centred area inside the band." },
        BlockField.Checkbox("fullWidth", "Content spans the full width") with { Modes = DocumentMode.Web },
        BlockField.Image("backgroundImage", "Background image") with { Modes = DocumentMode.Web },
        BlockField.Text("minHeight", "Minimum height") with { Placeholder = "e.g. 400px", Modes = DocumentMode.Web },
    ];

    public override IReadOnlyList<SlotDefinition> GetSlots(BlockNode node) => Slots;

    public override IReadOnlyCollection<string> SelfAppliedStyles(DocumentMode mode) =>
        mode == DocumentMode.Email ? EmailMarkup.CellStyles : [];

    public override BlockNode Create() => base.Create().Set(CommonStyles.Padding, "48px 16px");

    public override async ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string children = await context.RenderSlotAsync();
        if (context.IsEmail)
        {
            return $"""<table {EmailMarkup.Table} class="pk-section"><tr><td style="{Html.Encode(EmailMarkup.CellStyle(context, "24px"))}"{context.SlotAttributes()}>{children}</td></tr></table>""";
        }

        string outer = Css.Declarations(
            ("background-image", Css.UrlValue(context.Value("backgroundImage"))),
            ("min-height", context.Value("minHeight")));

        string inner = Css.Declarations(("max-width", context.Bool("fullWidth") ? "none" : context.Value("maxWidth")));
        return $"""<section class="pk-section" style="{Html.Encode(outer)}"><div class="pk-container" style="{Html.Encode(inner)}"{context.SlotAttributes()}>{children}</div></section>""";
    }
}
