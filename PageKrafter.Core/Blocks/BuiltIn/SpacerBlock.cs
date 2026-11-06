namespace PageKrafter.Blocks.BuiltIn;

/// <summary>
/// Represents the spacer block.
/// </summary>
public sealed class SpacerBlock : BlockDefinition
{
    /// <inheritdoc/>
    public override string Type => BlockTypes.Spacer;

    /// <inheritdoc/>
    public override string Label => "Spacer";

    /// <inheritdoc/>
    public override string Icon => BlockIcons.Spacer;

    /// <inheritdoc/>
    public override int Order => 4;

    /// <inheritdoc/>
    public override DocumentMode Modes => DocumentMode.All;

    /// <inheritdoc/>
    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Text("height", "Height", "32px"),
    ];

    /// <inheritdoc/>
    public override ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string height = context.Style("height");
        return ValueTask.FromResult(context.IsEmail
            ? $"""<div class="pk-spacer" style="{Html.Encode(Css.Declarations(("height", height), ("line-height", height), ("font-size", "1px")))}">&nbsp;</div>"""
            : $"""<div class="pk-spacer" style="{Html.Encode(Css.Declarations(("height", height)))}"></div>""");
    }
}