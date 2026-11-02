using System.Text.RegularExpressions;

namespace PageKrafter.Blocks.BuiltIn;

public sealed partial class VideoBlock : BlockDefinition
{
    public override string Type => BlockTypes.Video;

    public override string Label => "Video";

    public override string Category => BlockCategories.Media;

    public override string Icon => BlockIcons.Video;

    public override int Order => 1;

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Url("url", "Video link") with { HelpText = "A YouTube or Vimeo link, or a direct link to an .mp4 or .webm file." },
        BlockField.Select("aspect", "Aspect ratio", "16:9", ("16:9", "16:9"), ("4:3", "4:3"), ("1:1", "Square"), ("21:9", "21:9")),
    ];

    public static string? EmbedUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var youTube = YouTubePattern().Match(url);
        if (youTube.Success)
        {
            return $"https://www.youtube-nocookie.com/embed/{youTube.Groups[1].Value}";
        }

        var vimeo = VimeoPattern().Match(url);
        return vimeo.Success ? $"https://player.vimeo.com/video/{vimeo.Groups[1].Value}" : null;
    }

    public override ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string url = Html.SafeUrl(context.Value("url"));
        string padding = context.Value("aspect") switch
        {
            "4:3" => "75",
            "1:1" => "100",
            "21:9" => "42.86",
            _ => "56.25",
        };

        string? embed = EmbedUrl(url);
        string player;
        if (embed is not null)
        {
            player = $"""<iframe src="{Html.Encode(embed)}" title="Video" loading="lazy" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>""";
        }
        else if (FilePattern().IsMatch(url))
        {
            player = $"""<video src="{Html.Encode(url)}" controls preload="metadata"></video>""";
        }
        else
        {
            return ValueTask.FromResult(context.IsEditor
                ? """<div class="pk-missing">Paste a YouTube or Vimeo link in the settings.</div>"""
                : string.Empty);
        }

        return ValueTask.FromResult($"""<div class="pk-video" style="padding-bottom:{padding}%">{player}</div>""");
    }

    [GeneratedRegex(@"(?:youtube(?:-nocookie)?\.com/(?:watch\?(?:.*&)?v=|embed/|shorts/|live/)|youtu\.be/)([A-Za-z0-9_-]{6,20})", RegexOptions.IgnoreCase)]
    private static partial Regex YouTubePattern();

    [GeneratedRegex(@"vimeo\.com/(?:video/)?(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex VimeoPattern();

    [GeneratedRegex(@"\.(mp4|webm|ogg)(\?.*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex FilePattern();
}
