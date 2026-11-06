namespace PageKrafter.Blocks.BuiltIn;

/// <summary>
/// Represents the heading block.
/// </summary>
public sealed class HeadingBlock : BlockDefinition
{
    /// <inheritdoc/>
    public override string Type => BlockTypes.Heading;

    /// <inheritdoc/>
    public override string Label => "Heading";

    /// <inheritdoc/>
    public override string Icon => BlockIcons.Heading;

    /// <inheritdoc/>
    public override int Order => 0;

    /// <inheritdoc/>
    public override DocumentMode Modes => DocumentMode.All;

    /// <inheritdoc/>
    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Text("text", "Text", "Your heading here") with { HelpText = "You can also double-click the heading on the page.", AcceptsTokens = true },
        BlockField.Select("level", "Level", "h2", ("h1", "Heading 1"), ("h2", "Heading 2"), ("h3", "Heading 3"), ("h4", "Heading 4"), ("h5", "Heading 5"), ("h6", "Heading 6")),
    ];

    /// <inheritdoc/>
    public override string Summarize(BlockNode node) => Html.Truncate(node.Get("text") ?? string.Empty, 32);

    /// <inheritdoc/>
    public override ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string level = context.Value("level") is "h1" or "h2" or "h3" or "h4" or "h5" or "h6" ? context.Value("level") : "h2";
        if (context.IsEmail)
        {
            string size = level switch
            {
                "h1" => "30px",
                "h2" => "24px",
                "h3" => "20px",
                "h4" => "18px",
                "h5" => "16px",
                _ => "14px",
            };

            return ValueTask.FromResult($"""<{level} class="pk-heading" style="margin:0 0 12px;font-size:{size};line-height:1.25;font-weight:bold"{context.EditableAttributes("text")}>{context.Text("text")}</{level}>""");
        }

        return ValueTask.FromResult($"""<{level} class="pk-heading"{context.EditableAttributes("text")}>{context.Text("text")}</{level}>""");
    }
}