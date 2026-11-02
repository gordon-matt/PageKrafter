namespace PageKrafter.Blocks.BuiltIn;

public sealed class TextBlock : BlockDefinition
{
    public override string Type => BlockTypes.Text;

    public override string Label => "Text";

    public override string Icon => BlockIcons.Text;

    public override int Order => 1;

    public override DocumentMode Modes => DocumentMode.All;

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.RichText("content", "Text", "<p>Start writing here. Double-click this text on the page to edit it in place.</p>") with
        {
            HelpText = "Double-click the text on the page to edit it with formatting.",
        },
    ];

    public override string Summarize(BlockNode node) => Html.Truncate(Html.StripTags(node.Get("content")), 32);

    public override ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string style = context.IsEmail ? " style=\"font-size:16px;line-height:1.6\"" : string.Empty;
        return ValueTask.FromResult($"""<div class="pk-text"{style}{context.EditableAttributes("content", rich: true)}>{context.RichText("content")}</div>""");
    }
}
