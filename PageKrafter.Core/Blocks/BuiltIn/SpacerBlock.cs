namespace PageKrafter.Blocks.BuiltIn;

public sealed class SpacerBlock : BlockDefinition
{
    public override string Type => BlockTypes.Spacer;

    public override string Label => "Spacer";

    public override string Icon => BlockIcons.Spacer;

    public override int Order => 4;

    public override DocumentMode Modes => DocumentMode.All;

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Text("height", "Height", "32px"),
    ];

    public override ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string height = context.Style("height");
        return ValueTask.FromResult(context.IsEmail
            ? $"""<div class="pk-spacer" style="{Html.Encode(Css.Declarations(("height", height), ("line-height", height), ("font-size", "1px")))}">&nbsp;</div>"""
            : $"""<div class="pk-spacer" style="{Html.Encode(Css.Declarations(("height", height)))}"></div>""");
    }
}
