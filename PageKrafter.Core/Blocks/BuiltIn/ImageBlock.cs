namespace PageKrafter.Blocks.BuiltIn;

/// <summary>
/// Represents the image block.
/// </summary>
public sealed class ImageBlock : BlockDefinition
{
    private const string Placeholder =
        "data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' width='640' height='360' viewBox='0 0 640 360'%3E%3Crect width='640' height='360' fill='%23e2e8f0'/%3E%3Cpath d='M250 230l50-60 40 45 30-30 60 45z' fill='%2394a3b8'/%3E%3Ccircle cx='275' cy='140' r='18' fill='%2394a3b8'/%3E%3C/svg%3E";

    /// <inheritdoc/>
    public override string Type => BlockTypes.Image;

    /// <inheritdoc/>
    public override string Label => "Image";

    /// <inheritdoc/>
    public override string Category => BlockCategories.Media;

    /// <inheritdoc/>
    public override string Icon => BlockIcons.Image;

    /// <inheritdoc/>
    public override int Order => 0;

    /// <inheritdoc/>
    public override DocumentMode Modes => DocumentMode.All;

    /// <inheritdoc/>
    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Image("src", "Image"),
        BlockField.Text("alt", "Alternative text") with { HelpText = "Describes the image for screen readers and when images are blocked." },
        BlockField.Text("width", "Width") with { Placeholder = "e.g. 100% or 320px" },
        BlockField.Url("link", "Link"),
        BlockField.Checkbox("newTab", "Open link in a new tab"),
        BlockField.Text("caption", "Caption") with { Modes = DocumentMode.Web },
    ];

    /// <inheritdoc/>
    public override BlockNode Create() => base.Create().Set(CommonStyles.Align, "center");

    /// <inheritdoc/>
    public override string Summarize(BlockNode node) => Html.Truncate(node.Get("alt") is { Length: > 0 } alt ? alt : Path.GetFileName(node.Get("src") ?? string.Empty), 32);

    /// <inheritdoc/>
    public override ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string src = Html.SafeUrl(context.Value("src"), allowDataImage: true);
        if (src.Length == 0)
        {
            if (!context.IsEditor)
            {
                return ValueTask.FromResult(string.Empty);
            }

            src = Placeholder;
        }

        string width = context.Style("width");
        string alt = context.Text("alt");
        string link = Html.SafeUrl(context.Value("link"));
        string target = context.Bool("newTab") ? " target=\"_blank\" rel=\"noopener\"" : string.Empty;

        string image;
        if (context.IsEmail)
        {
            int? pixels = width.EndsWith("px", StringComparison.OrdinalIgnoreCase) ? Css.Pixels(width) : null;
            string widthAttribute = pixels is int value ? $" width=\"{value}\"" : string.Empty;
            string style = Css.Declarations(("display", "inline-block"), ("max-width", "100%"), ("height", "auto"), ("border", "0"), ("width", width));
            image = $"""<img src="{Html.Encode(src)}" alt="{alt}"{widthAttribute} style="{Html.Encode(style)}">""";
        }
        else
        {
            string style = Css.Declarations(("width", width));
            image = $"""<img class="pk-img" src="{Html.Encode(src)}" alt="{alt}" loading="lazy" style="{Html.Encode(style)}">""";
        }

        if (link.Length > 0)
        {
            image = $"""<a href="{Html.Encode(link)}"{target}>{image}</a>""";
        }

        if (context.IsEmail)
        {
            return ValueTask.FromResult($"""<div class="pk-image">{image}</div>""");
        }

        string caption = context.Value("caption").Length > 0 ? $"<figcaption>{context.Text("caption")}</figcaption>" : string.Empty;
        return ValueTask.FromResult($"""<figure class="pk-image">{image}{caption}</figure>""");
    }
}