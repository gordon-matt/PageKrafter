namespace PageKrafter.Blocks.BuiltIn;

/// <summary>
/// Represents the text block.
/// </summary>
public sealed class TextBlock : BlockDefinition
{
    /// <inheritdoc/>
    public override string Type => BlockTypes.Text;

    /// <inheritdoc/>
    public override string Label => "Text";

    /// <inheritdoc/>
    public override string Icon => BlockIcons.Text;

    /// <inheritdoc/>
    public override int Order => 1;

    /// <inheritdoc/>
    public override DocumentMode Modes => DocumentMode.All;

    /// <inheritdoc/>
    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.RichText("content", "Text", "<p>Start writing here. Double-click this text on the page to edit it in place.</p>") with
        {
            HelpText = "Double-click the text on the page to edit it with formatting.",
            AcceptsTokens = true,
        },
    ];

    /// <inheritdoc/>
    public override string Summarize(BlockNode node) => Html.Truncate(Html.StripTags(node.Get("content")), 32);

    /// <inheritdoc/>
    public override ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string style = context.IsEmail ? " style=\"font-size:16px;line-height:1.6\"" : string.Empty;
        return ValueTask.FromResult($"""<div class="pk-text"{style}{context.EditableAttributes("content", rich: true)}>{context.RichText("content")}</div>""");
    }
}