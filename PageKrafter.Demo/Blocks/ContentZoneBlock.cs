namespace PageKrafter.Demo.Blocks;

/// <summary>
/// A sample host block: a region whose contents come from the host application rather than the
/// page. Exported HTML holds a <c>[[ContentZone:Name]]</c> token, which the host replaces with
/// the zone's HTML when the page is shown.
/// </summary>
public sealed class ContentZoneBlock(IZoneCatalog zones) : BlockDefinition
{
    public const string TypeName = "cms.content-zone";

    public override string Type => TypeName;

    public override string Label => "Content zone";

    public override string Category => "CMS";

    public override string Icon => BlockIcons.Zone;

    public override string Description => "Shows the content blocks assigned to a CMS zone.";

    // The token is replaced by the CMS at display time, so there is no element to style.
    public override bool UsesCommonStyles => false;

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        new BlockField
        {
            Name = "zone",
            Label = "Zone",
            Editor = FieldEditors.Select,
            HasDynamicOptions = true,
            HelpText = "Add and order the zone's content blocks in the CMS.",
        },
    ];

    public static string Token(string zone) => $"[[ContentZone:{zone}]]";

    public override async Task<IReadOnlyList<FieldOption>> GetOptionsAsync(BlockField field, BlockNode node, CancellationToken cancellationToken = default)
    {
        var names = await zones.ListAsync(cancellationToken);
        return [new FieldOption(string.Empty, "Choose a zone…"), .. names.Select(name => new FieldOption(name, name))];
    }

    public override string Summarize(BlockNode node) => node.Get("zone") ?? string.Empty;

    public override async ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string zone = context.Value("zone").Replace("]]", string.Empty).Trim();
        if (!context.IsEditor)
        {
            return zone.Length == 0 ? string.Empty : Token(zone);
        }

        if (zone.Length == 0)
        {
            return """<div class="pk-missing">Choose a zone for this block in the settings.</div>""";
        }

        string preview = await zones.RenderAsync(zone, context.CancellationToken);
        return $"""
            <div style="position:relative;padding:28px 12px 12px;border:1px dashed #8b5cf6;border-radius:6px;background:#faf5ff">
            <span style="position:absolute;top:6px;left:10px;color:#6d28d9;font:600 11px system-ui,sans-serif;letter-spacing:.04em;text-transform:uppercase">Zone: {Html.Encode(zone)}</span>
            {context.Sanitizer.Sanitize(preview)}
            </div>
            """;
    }
}