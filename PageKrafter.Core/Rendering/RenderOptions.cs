namespace PageKrafter.Rendering;

public enum RenderPurpose
{
    Export,
    Editor,
}

public sealed record RenderOptions
{
    public static RenderOptions Export { get; } = new();

    public static RenderOptions Editor { get; } = new() { Purpose = RenderPurpose.Editor, IncludeBaseStyles = false };

    public RenderPurpose Purpose { get; init; } = RenderPurpose.Export;

    /// <summary>
    /// Prepends a small style block with the layout rules (column stacking, hide on mobile). Leave it on
    /// when the HTML is stored in a CMS field, because the host page has no PageKrafter stylesheet.
    /// </summary>
    public bool IncludeBaseStyles { get; init; } = true;
}