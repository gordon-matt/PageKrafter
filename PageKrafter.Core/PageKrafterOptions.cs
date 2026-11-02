namespace PageKrafter;

public sealed class PageKrafterOptions
{
    /// <summary>Built-in or host block types to leave out, such as <c>html</c> when editors must not paste raw markup.</summary>
    public HashSet<string> DisabledBlocks { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Stylesheets from the host site loaded into the web canvas, so blocks preview with the real
    /// theme. They are not added to exported HTML because the host page already has them.
    /// Email canvases skip them because email clients never see site CSS.
    /// </summary>
    public List<string> CanvasStyles { get; } = [];

    /// <summary>Extra markup for the canvas head, such as font links.</summary>
    public string? CanvasHeadHtml { get; set; }

    public int HistoryLimit { get; set; } = 100;

    /// <summary>Maps an editor name from <see cref="Blocks.BlockField.Editor"/> to a Razor component deriving from <c>FieldEditorBase</c>.</summary>
    public Dictionary<string, Type> FieldEditorComponents { get; } = new(StringComparer.Ordinal);
}
