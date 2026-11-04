namespace PageKrafter.Demo.Blocks;

/// <summary>A host block written as a Razor component, with a slot for child blocks and a custom field editor.</summary>
public sealed class CalloutBlock : ComponentBlockDefinition<CalloutView>
{
    public const string ToneEditor = "demo.callout-tone";

    private static readonly IReadOnlyList<SlotDefinition> Slots = [new(SlotDefinition.Default, "the callout")];

    public static IReadOnlyList<CalloutTone> Tones { get; } =
    [
        new("info", "Info", "#2563eb", "#eff6ff"),
        new("success", "Success", "#16a34a", "#f0fdf4"),
        new("warning", "Warning", "#d97706", "#fffbeb"),
        new("danger", "Danger", "#dc2626", "#fef2f2"),
    ];

    public override string Type => "demo.callout";

    public override string Label => "Callout";

    public override string Category => "Demo";

    public override string Icon => BlockIcons.Callout;

    public override string Description => "A highlighted note that can hold any blocks.";

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Text("title", "Title", "Good to know"),
        new BlockField { Name = "tone", Label = "Tone", Editor = ToneEditor, DefaultValue = "info" },
    ];

    public override IReadOnlyList<SlotDefinition> GetSlots(BlockNode node) => Slots;

    public override BlockNode Create() =>
        base.Create().Add(Nodes.Text("<p>Put any blocks in here. Drag more in from the palette.</p>"));

    public override string Summarize(BlockNode node) => node.Get("title") ?? string.Empty;

    public static CalloutTone ToneFor(string? key) => Tones.FirstOrDefault(tone => tone.Key == key) ?? Tones[0];
}

public sealed record CalloutTone(string Key, string Label, string Border, string Background);