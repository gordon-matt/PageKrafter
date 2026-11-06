namespace PageKrafter.Blocks.BuiltIn;

/// <summary>
/// Represents the divider block.
/// </summary>
public sealed class DividerBlock : BlockDefinition
{
    /// <inheritdoc/>
    public override string Type => BlockTypes.Divider;

    /// <inheritdoc/>
    public override string Label => "Divider";

    /// <inheritdoc/>
    public override string Icon => BlockIcons.Divider;

    /// <inheritdoc/>
    public override int Order => 3;

    /// <inheritdoc/>
    public override DocumentMode Modes => DocumentMode.All;

    /// <inheritdoc/>
    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Color("lineColor", "Line color", "#d0d5dd", FieldGroups.Content),
        BlockField.Text("thickness", "Thickness", "1px"),
        BlockField.Select("lineStyle", "Style", "solid", ("solid", "Solid"), ("dashed", "Dashed"), ("dotted", "Dotted")),
        BlockField.Text("width", "Width", "100%"),
        BlockField.Text("spacing", "Space above and below", "16px"),
    ];

    /// <inheritdoc/>
    public override ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string lineStyle = context.Value("lineStyle") is "dashed" or "dotted" ? context.Value("lineStyle") : "solid";
        string border = $"{context.Style("thickness")} {lineStyle} {context.Style("lineColor")}";
        if (context.IsEmail)
        {
            string cell = Css.Declarations(("padding", $"{context.Style("spacing")} 0"));
            string line = Css.Declarations(("border-top", border), ("font-size", "1px"), ("line-height", "1px"), ("width", context.Value("width")), ("margin", "0 auto"));
            return ValueTask.FromResult($"""<table {EmailMarkup.Table} class="pk-divider"><tr><td style="{Html.Encode(cell)}"><div style="{Html.Encode(line)}">&nbsp;</div></td></tr></table>""");
        }

        string style = Css.Declarations(
            ("border", "0"),
            ("border-top", border),
            ("margin", $"{context.Style("spacing")} auto"),
            ("width", context.Value("width")));

        return ValueTask.FromResult($"""<hr class="pk-divider" style="{Html.Encode(style)}">""");
    }
}