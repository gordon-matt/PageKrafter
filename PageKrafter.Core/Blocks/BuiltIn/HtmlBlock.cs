namespace PageKrafter.Blocks.BuiltIn;

/// <summary>Raw markup for things no block covers. It is sanitized, so scripts and event handlers are removed. Disable it with <see cref="PageKrafterOptions.DisabledBlocks"/>.</summary>
public sealed class HtmlBlock : BlockDefinition
{
    /// <inheritdoc/>
    public override string Type => BlockTypes.Html;

    /// <inheritdoc/>
    public override string Label => "HTML";

    /// <inheritdoc/>
    public override string Category => BlockCategories.Advanced;

    /// <inheritdoc/>
    public override string Icon => BlockIcons.Html;

    /// <inheritdoc/>
    public override string Description => "Custom markup. Scripts are removed.";

    /// <inheritdoc/>
    public override DocumentMode Modes => DocumentMode.All;

    /// <inheritdoc/>
    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Html("html", "HTML", """<div style="padding:16px;border:1px dashed #94a3b8;text-align:center">Custom HTML</div>""") with
        {
            HelpText = "Scripts, iframes and event handlers are removed.",
        },
    ];

    /// <inheritdoc/>
    public override ValueTask<string> RenderAsync(BlockRenderContext context) =>
        ValueTask.FromResult($"""<div class="pk-html">{context.RichText("html")}</div>""");
}