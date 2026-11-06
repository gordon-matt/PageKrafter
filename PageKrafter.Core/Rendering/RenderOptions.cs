namespace PageKrafter.Rendering;

/// <summary>
/// Specifies the render purpose.
/// </summary>
public enum RenderPurpose
{
    /// <summary>
    /// The export value.
    /// </summary>
    Export,
    /// <summary>
    /// The editor value.
    /// </summary>
    Editor,
}

/// <summary>
/// Represents the render options.
/// </summary>
public sealed record RenderOptions
{
    /// <summary>
    /// Gets the export.
    /// </summary>
    public static RenderOptions Export { get; } = new();

    /// <summary>
    /// Gets the editor.
    /// </summary>
    public static RenderOptions Editor { get; } = new() { Purpose = RenderPurpose.Editor, IncludeBaseStyles = false };

    /// <summary>
    /// Gets or sets the purpose.
    /// </summary>
    public RenderPurpose Purpose { get; init; } = RenderPurpose.Export;

    /// <summary>
    /// Prepends a small style block with the layout rules (column stacking, hide on mobile). Leave it on
    /// when the HTML is stored in a CMS field, because the host page has no PageKrafter stylesheet.
    /// </summary>
    public bool IncludeBaseStyles { get; init; } = true;
}