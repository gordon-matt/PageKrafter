namespace PageKrafter.Blocks.BuiltIn;

public sealed class ButtonBlock : BlockDefinition
{
    public override string Type => BlockTypes.Button;

    public override string Label => "Button";

    public override string Icon => BlockIcons.Button;

    public override int Order => 2;

    public override DocumentMode Modes => DocumentMode.All;

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Text("text", "Text", "Learn more"),
        BlockField.Url("url", "Link", "#"),
        BlockField.Checkbox("newTab", "Open in a new tab"),
        BlockField.Select("size", "Size", "md", ("sm", "Small"), ("md", "Medium"), ("lg", "Large")),
        BlockField.Checkbox("fullWidth", "Full width"),
        BlockField.Color("buttonColor", "Button color", "#2563eb"),
        BlockField.Color("buttonTextColor", "Button text color", "#ffffff"),
        BlockField.Text("buttonRadius", "Button corner radius", "6px", FieldGroups.Style),
    ];

    public override string Summarize(BlockNode node) => Html.Truncate(node.Get("text") ?? string.Empty, 32);

    public override ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string href = Html.Encode(Html.SafeUrl(context.Value("url")) is { Length: > 0 } url ? url : "#");
        string target = context.Bool("newTab") ? " target=\"_blank\" rel=\"noopener\"" : string.Empty;
        string background = context.Style("buttonColor");
        string color = context.Style("buttonTextColor");
        string radius = context.Style("buttonRadius");
        (string padding, string fontSize) = context.Value("size") switch
        {
            "sm" => ("8px 14px", "14px"),
            "lg" => ("16px 30px", "18px"),
            _ => ("12px 22px", "16px"),
        };

        bool fullWidth = context.Bool("fullWidth");
        string editable = context.EditableAttributes("text");

        if (context.IsEmail)
        {
            string align = context.Node.Get(CommonStyles.Align) is "left" or "right" ? context.Node.Get(CommonStyles.Align)! : "center";
            string link = Css.Declarations(
                ("display", "block"),
                ("padding", padding),
                ("color", color),
                ("font-size", fontSize),
                ("font-weight", "bold"),
                ("text-decoration", "none"),
                ("border-radius", radius));

            string tableWidth = fullWidth ? " width=\"100%\"" : string.Empty;
            return ValueTask.FromResult(
                $"""<div class="pk-button"><table role="presentation" cellpadding="0" cellspacing="0" border="0" align="{align}"{tableWidth}><tr><td align="center" bgcolor="{Html.Encode(background)}" style="{Html.Encode(Css.Declarations(("border-radius", radius), ("background-color", background)))}"><a href="{href}"{target} style="{Html.Encode(link)}"{editable}>{context.Text("text")}</a></td></tr></table></div>""");
        }

        string style = Css.Declarations(
            ("background-color", background),
            ("color", color),
            ("padding", padding),
            ("font-size", fontSize),
            ("border-radius", radius),
            ("display", fullWidth ? "block" : string.Empty),
            ("text-align", fullWidth ? "center" : string.Empty));

        return ValueTask.FromResult($"""<div class="pk-button"><a class="pk-btn" href="{href}"{target} style="{Html.Encode(style)}"{editable}>{context.Text("text")}</a></div>""");
    }
}
