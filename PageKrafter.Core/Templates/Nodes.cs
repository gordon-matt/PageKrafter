namespace PageKrafter.Templates;

/// <summary>Builds block trees in code, for template providers and tests.</summary>
public static class Nodes
{
    /// <summary>
    /// Creates the value.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="props">The props.</param>
    /// <returns>The block node.</returns>
    public static BlockNode Create(string type, params (string Name, string Value)[] props)
    {
        var node = new BlockNode { Type = type };
        foreach (var (name, value) in props)
        {
            node.Props[name] = value;
        }

        return node;
    }

    /// <summary>
    /// Gets the page.
    /// </summary>
    /// <param name="children">The children.</param>
    /// <returns>The page.</returns>
    public static BlockNode Page(params BlockNode[] children) =>
        new BlockNode { Id = PageDocument.RootId, Type = BlockTypes.Page }.Add(children);

    /// <summary>
    /// Gets the section.
    /// </summary>
    /// <param name="children">The children.</param>
    /// <returns>The section.</returns>
    public static BlockNode Section(params BlockNode[] children) =>
        Create(BlockTypes.Section, (CommonStyles.Padding, "48px 16px")).Add(children);

    /// <summary>
    /// Gets the box.
    /// </summary>
    /// <param name="children">The children.</param>
    /// <returns>The box.</returns>
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

    /// <summary>
    /// Gets the heading.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="level">The level.</param>
    /// <returns>The heading.</returns>
    public static BlockNode Heading(string text, string level = "h2") => Create(BlockTypes.Heading, ("text", text), ("level", level));

    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns>The text.</returns>
    public static BlockNode Text(string html) => Create(BlockTypes.Text, ("content", html));

    /// <summary>
    /// Gets the button.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="url">The URL.</param>
    /// <returns>The button.</returns>
    public static BlockNode Button(string text, string url = "#") => Create(BlockTypes.Button, ("text", text), ("url", url));

    /// <summary>
    /// Gets the image.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <param name="alt">The alt.</param>
    /// <returns>The image.</returns>
    public static BlockNode Image(string src = "", string alt = "") => Create(BlockTypes.Image, ("src", src), ("alt", alt), (CommonStyles.Align, "center"));

    /// <summary>
    /// Gets the spacer.
    /// </summary>
    /// <param name="height">The height.</param>
    /// <returns>The spacer.</returns>
    public static BlockNode Spacer(string height = "32px") => Create(BlockTypes.Spacer, ("height", height));

    /// <summary>
    /// Gets the divider.
    /// </summary>
    /// <returns>The divider.</returns>
    public static BlockNode Divider() => Create(BlockTypes.Divider);

    /// <summary>
    /// Adds the value.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="children">The children.</param>
    /// <returns>The block node.</returns>
    public static BlockNode Add(this BlockNode node, params BlockNode[] children) => node.AddTo(SlotDefinition.Default, children);

    /// <summary>
    /// Adds to.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="slot">The slot.</param>
    /// <param name="children">The children.</param>
    /// <returns>The block node.</returns>
    public static BlockNode AddTo(this BlockNode node, string slot, params BlockNode[] children)
    {
        node.Slot(slot).AddRange(children);
        return node;
    }

    /// <summary>
    /// Returns a copy with the value.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <returns>The block node.</returns>
    public static BlockNode With(this BlockNode node, string name, string value) => node.Set(name, value);
}