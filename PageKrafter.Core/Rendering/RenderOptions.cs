namespace PageKrafter.Rendering;

/// <summary>Whether HTML is being stored or shown in the canvas.</summary>
public enum RenderPurpose
{
    /// <summary>HTML stored or sent by the host. Editor attributes are omitted.</summary>
    Export,

    /// <summary>HTML for the canvas. Selection, drop, and inline-edit attributes are included.</summary>
    Editor,
}

/// <summary>Chooses export or editor rendering, and whether the base style block is included.</summary>
public sealed record RenderOptions
{
    /// <summary>Options for stored or sent HTML. Base styles are included.</summary>
    public static RenderOptions Export { get; } = new();

    /// <summary>Options for the canvas. Base styles are left out because the canvas document already includes them.</summary>
    public static RenderOptions Editor { get; } = new() { Purpose = RenderPurpose.Editor, IncludeBaseStyles = false };

    /// <summary>Whether the HTML is for storage or for the canvas. The default is export.</summary>
    public RenderPurpose Purpose { get; init; } = RenderPurpose.Export;

    /// <summary>
    /// Prepends a small style block with the layout rules (column stacking, hide on mobile). Leave it on
    /// when the HTML is stored in a CMS field, because the host page has no PageKrafter stylesheet.
    /// </summary>
    public bool IncludeBaseStyles { get; init; } = true;
}