namespace PageKrafter.Templates;

/// <summary>Builds block trees in code, for template providers and tests.</summary>
public static class Nodes
{
    public static BlockNode Create(string type, params (string Name, string Value)[] props)
    {
        var node = new BlockNode { Type = type };
        foreach (var (name, value) in props)
        {
            node.Props[name] = value;
        }

        return node;
    }

    public static BlockNode Page(params BlockNode[] children) =>
        new BlockNode { Id = PageDocument.RootId, Type = BlockTypes.Page }.Add(children);

    public static BlockNode Section(params BlockNode[] children) =>
        Create(BlockTypes.Section, (CommonStyles.Padding, "48px 16px")).Add(children);

    public static BlockNode Box(params BlockNode[] children) =>
        Create(BlockTypes.Container, (CommonStyles.Padding, "16px")).Add(children);

    /// <summary>One array of blocks per column. <paramref name="widths"/> is a ratio such as <c>1:2</c>, or empty for equal columns.</summary>
    public static BlockNode Columns(string widths, params BlockNode[][] columns)
    {
        var node = Create(BlockTypes.Columns, ("count", columns.Length.ToString(CultureInfo.InvariantCulture)), ("gap", "24px"), ("stackOnMobile", "true"));
        if (widths.Length > 0)
        {
            node.Props["widths"] = widths;
        }

        for (int i = 0; i < columns.Length; i++)
        {
            node.Slot(ColumnsBlock.SlotName(i)).AddRange(columns[i]);
        }

        return node;
    }

    public static BlockNode Heading(string text, string level = "h2") => Create(BlockTypes.Heading, ("text", text), ("level", level));

    public static BlockNode Text(string html) => Create(BlockTypes.Text, ("content", html));

    public static BlockNode Button(string text, string url = "#") => Create(BlockTypes.Button, ("text", text), ("url", url));

    public static BlockNode Image(string src = "", string alt = "") => Create(BlockTypes.Image, ("src", src), ("alt", alt), (CommonStyles.Align, "center"));

    public static BlockNode Spacer(string height = "32px") => Create(BlockTypes.Spacer, ("height", height));

    public static BlockNode Divider() => Create(BlockTypes.Divider);

    public static BlockNode Add(this BlockNode node, params BlockNode[] children) => node.AddTo(SlotDefinition.Default, children);

    public static BlockNode AddTo(this BlockNode node, string slot, params BlockNode[] children)
    {
        node.Slot(slot).AddRange(children);
        return node;
    }

    public static BlockNode With(this BlockNode node, string name, string value) => node.Set(name, value);
}