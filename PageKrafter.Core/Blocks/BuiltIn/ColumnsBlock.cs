namespace PageKrafter.Blocks.BuiltIn;

/// <summary>
/// Represents the columns block.
/// </summary>
public sealed class ColumnsBlock : BlockDefinition
{
    /// <summary>
    /// The max columns constant.
    /// </summary>
    public const int MaxColumns = 6;

    private static readonly Dictionary<int, (string Value, string Label)[]> Presets = new()
    {
        [2] = [("1:1", "Equal"), ("1:2", "Narrow, wide"), ("2:1", "Wide, narrow"), ("1:3", "Sidebar left"), ("3:1", "Sidebar right")],
        [3] = [("1:1:1", "Equal"), ("1:2:1", "Wide centre"), ("2:1:1", "Wide left"), ("1:1:2", "Wide right")],
        [4] = [("1:1:1:1", "Equal"), ("2:1:1:2", "Wide outsides"), ("1:2:2:1", "Wide insides")],
    };

    /// <inheritdoc/>
    public override string Type => BlockTypes.Columns;

    /// <inheritdoc/>
    public override string Label => "Columns";

    /// <inheritdoc/>
    public override string Category => BlockCategories.Layout;

    /// <inheritdoc/>
    public override string Icon => BlockIcons.Columns;

    /// <inheritdoc/>
    public override string Description => "Side-by-side columns that stack on phones.";

    /// <inheritdoc/>
    public override int Order => 1;

    /// <inheritdoc/>
    public override DocumentMode Modes => DocumentMode.All;

    /// <inheritdoc/>
    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.Select("count", "Columns", "2", ("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5"), ("6", "6")),
        new BlockField
        {
            Name = "widths",
            Label = "Widths",
            Editor = FieldEditors.Select,
            HasDynamicOptions = true,
            OptionsDependOn = ["count"],
        },
        BlockField.Text("gap", "Gap", "24px"),
        BlockField.Select("verticalAlign", "Vertical alignment", "stretch", ("stretch", "Stretch"), ("top", "Top"), ("center", "Middle"), ("bottom", "Bottom")),
        BlockField.Checkbox("stackOnMobile", "Stack on mobile", defaultValue: true) with { Modes = DocumentMode.Web },
    ];

    /// <summary>
    /// Counts of.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The int.</returns>
    public static int CountOf(BlockNode node) =>
        int.TryParse(node.Get("count"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) ? Math.Clamp(count, 1, MaxColumns) : 2;

    /// <summary>
    /// Gets the slot name.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The slot name.</returns>
    public static string SlotName(int index) => $"col-{index + 1}";

    /// <summary>Reads a ratio like <c>1:2</c>. Anything that doesn't match the column count gives equal widths.</summary>
    public static decimal[] ParseRatios(string? widths, int count)
    {
        string[] parts = (widths ?? string.Empty).Split([':', ',', ' '], StringSplitOptions.RemoveEmptyEntries);
        decimal[] ratios = parts
            .Select(part => decimal.TryParse(part, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value) && value > 0 ? value : 0)
            .ToArray();

        return ratios.Length == count && ratios.All(ratio => ratio > 0) ? ratios : Enumerable.Repeat(1m, count).ToArray();
    }

    /// <inheritdoc/>
    public override IReadOnlyList<SlotDefinition> GetSlots(BlockNode node) =>
        Enumerable.Range(0, CountOf(node)).Select(index => new SlotDefinition(SlotName(index), $"Column {index + 1}")).ToArray();

    /// <inheritdoc/>
    public override Task<IReadOnlyList<FieldOption>> GetOptionsAsync(BlockField field, BlockNode node, CancellationToken cancellationToken = default)
    {
        if (field.Name != "widths")
        {
            return base.GetOptionsAsync(field, node, cancellationToken);
        }

        int count = CountOf(node);
        var options = new List<FieldOption> { new(string.Empty, "Equal") };
        if (Presets.TryGetValue(count, out var presets))
        {
            options.AddRange(presets.Where(preset => preset.Label != "Equal").Select(preset => new FieldOption(preset.Value, $"{preset.Label} ({preset.Value})")));
        }

        string current = node.Get("widths") ?? string.Empty;
        if (current.Length > 0 && options.All(option => option.Value != current))
        {
            options.Add(new FieldOption(current, $"Custom ({current})"));
        }

        return Task.FromResult<IReadOnlyList<FieldOption>>(options);
    }

    /// <inheritdoc/>
    public override void OnPropertyChanged(BlockNode node, string name)
    {
        if (name != "count")
        {
            return;
        }

        int count = CountOf(node);
        for (int i = 0; i < count; i++)
        {
            node.Slot(SlotName(i));
        }

        // Blocks in removed columns move to the last remaining column instead of being lost.
        var last = node.Slot(SlotName(count - 1));
        foreach (string slot in node.Slots.Keys.Where(slot => !IsKeptSlot(slot, count)).ToList())
        {
            last.AddRange(node.Slots[slot]);
            node.Slots.Remove(slot);
        }

        if (node.Get("widths") is { Length: > 0 } widths && widths.Split([':', ',', ' '], StringSplitOptions.RemoveEmptyEntries).Length != count)
        {
            node.Props.Remove("widths");
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        int count = CountOf(context.Node);
        decimal[] ratios = ParseRatios(context.Value("widths"), count);
        decimal total = ratios.Sum();
        string align = context.Value("verticalAlign");

        var builder = new StringBuilder();
        if (context.IsEmail)
        {
            int halfGap = (Css.Pixels(context.Value("gap")) ?? 0) / 2;
            string valign = align switch
            {
                "center" => "middle",
                "bottom" => "bottom",
                _ => "top",
            };

            builder.Append($"""<table {EmailMarkup.Table} class="pk-columns"><tr>""");
            for (int i = 0; i < count; i++)
            {
                string percent = Format(Math.Round(ratios[i] / total * 100, 2));
                string padding = $"padding:0 {(i == count - 1 ? 0 : halfGap)}px 0 {(i == 0 ? 0 : halfGap)}px;";
                builder.Append($"""<td class="pk-email-col" width="{percent}%" valign="{valign}" style="width:{percent}%;{padding}"{context.SlotAttributes(SlotName(i))}>""");
                builder.Append(await context.RenderSlotAsync(SlotName(i)));
                builder.Append("</td>");
            }

            builder.Append("</tr></table>");
            return builder.ToString();
        }

        string alignItems = align switch
        {
            "top" => "flex-start",
            "center" => "center",
            "bottom" => "flex-end",
            _ => "stretch",
        };

        string style = Css.Declarations(("gap", context.Value("gap")), ("align-items", alignItems));
        string stack = context.Bool("stackOnMobile") ? " pk-stack" : string.Empty;
        builder.Append($"""<div class="pk-columns{stack}" style="{Html.Encode(style)}">""");
        for (int i = 0; i < count; i++)
        {
            string ratio = Format(ratios[i]);
            builder.Append($"""<div class="pk-column" style="flex:{ratio} {ratio} 0%"{context.SlotAttributes(SlotName(i))}>""");
            builder.Append(await context.RenderSlotAsync(SlotName(i)));
            builder.Append("</div>");
        }

        builder.Append("</div>");
        return builder.ToString();
    }

    private static bool IsKeptSlot(string slot, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (slot == SlotName(i))
            {
                return true;
            }
        }

        return false;
    }

    private static string Format(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}