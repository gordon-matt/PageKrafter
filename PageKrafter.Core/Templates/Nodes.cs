namespace PageKrafter.Templates;

/// <summary>Builds block trees in code, for template providers and tests.</summary>
public static class Nodes
{
    /// <summary>A block of the given type. Each pair is stored as a prop, including empty values.</summary>
    /// <param name="type">The block type key.</param>
    /// <param name="props">The prop names and values.</param>
    /// <returns>The block. It has a new id.</returns>
    public static BlockNode Create(string type, params (string Name, string Value)[] props)
    {
        var node = new BlockNode { Type = type };
        foreach (var (name, value) in props)
        {
            node.Props[name] = value;
        }

        return node;
    }

    /// <summary>A page block with the fixed root id and these children in its default slot.</summary>
    /// <param name="children">The top-level blocks.</param>
    /// <returns>The page block.</returns>
    public static BlockNode Page(params BlockNode[] children) =>
        new BlockNode { Id = PageDocument.RootId, Type = BlockTypes.Page }.Add(children);

    /// <summary>A section with 48 pixels of vertical padding and 16 pixels of horizontal padding.</summary>
    /// <param name="children">The blocks inside the section.</param>
    /// <returns>The section.</returns>
    public static BlockNode Section(params BlockNode[] children) =>
        Create(BlockTypes.Section, (CommonStyles.Padding, "48px 16px")).Add(children);

    /// <summary>A container with 16 pixels of padding.</summary>
    /// <param name="children">The blocks inside the container.</param>
    /// <returns>The container.</returns>
    public static BlockNode Box(params BlockNode[] children) =>
        Create(BlockTypes.Container, (CommonStyles.Padding, "16px")).Add(children);

    /// <summary>
    /// A columns block. One array of blocks per column. The columns stack on phones, and the gap is 24 pixels.
    /// <paramref name="widths"/> is a ratio such as <c>1:2</c>, or empty for equal columns.
    /// </summary>
    /// <param name="widths">The width ratio, or an empty string for equal columns.</param>
    /// <param name="columns">The blocks in each column, from left to right.</param>
    /// <returns>The columns block.</returns>
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

    /// <summary>A heading. The default level is h2.</summary>
    /// <param name="text">The heading text.</param>
    /// <param name="level">The level, such as <c>h1</c>. The default is <c>h2</c>.</param>
    /// <returns>The heading.</returns>
    public static BlockNode Heading(string text, string level = "h2") => Create(BlockTypes.Heading, ("text", text), ("level", level));

    /// <summary>A text block. The HTML is stored as written and sanitized when the page is rendered.</summary>
    /// <param name="html">The HTML.</param>
    /// <returns>The text block.</returns>
    public static BlockNode Text(string html) => Create(BlockTypes.Text, ("content", html));

    /// <summary>A button. The link defaults to <c>#</c>.</summary>
    /// <param name="text">The button label.</param>
    /// <param name="url">The link. The default is <c>#</c>.</param>
    /// <returns>The button.</returns>
    public static BlockNode Button(string text, string url = "#") => Create(BlockTypes.Button, ("text", text), ("url", url));

    /// <summary>An image, centered. Both the address and the alternative text default to empty.</summary>
    /// <param name="src">The image address.</param>
    /// <param name="alt">The alternative text.</param>
    /// <returns>The image.</returns>
    public static BlockNode Image(string src = "", string alt = "") => Create(BlockTypes.Image, ("src", src), ("alt", alt), (CommonStyles.Align, "center"));

    /// <summary>A spacer. The default height is 32 pixels.</summary>
    /// <param name="height">The height, such as <c>32px</c>.</param>
    /// <returns>The spacer.</returns>
    public static BlockNode Spacer(string height = "32px") => Create(BlockTypes.Spacer, ("height", height));

    /// <summary>A divider with the block's default line.</summary>
    /// <returns>The divider.</returns>
    public static BlockNode Divider() => Create(BlockTypes.Divider);

    /// <summary>Appends children to the default slot and returns the same block.</summary>
    /// <param name="node">The block that receives the children.</param>
    /// <param name="children">The children to append.</param>
    /// <returns>The same block.</returns>
    public static BlockNode Add(this BlockNode node, params BlockNode[] children) => node.AddTo(SlotDefinition.Default, children);

    /// <summary>Appends children to a slot, creating the slot when it is missing, and returns the same block.</summary>
    /// <param name="node">The block that receives the children.</param>
    /// <param name="slot">The slot name.</param>
    /// <param name="children">The children to append.</param>
    /// <returns>The same block.</returns>
    public static BlockNode AddTo(this BlockNode node, string slot, params BlockNode[] children)
    {
        node.Slot(slot).AddRange(children);
        return node;
    }

    /// <summary>Stores a prop on the block and returns the same block. A <see langword="null"/> value removes the prop.</summary>
    /// <param name="node">The block to change.</param>
    /// <param name="name">The prop name.</param>
    /// <param name="value">The text to store, or <see langword="null"/> to remove it.</param>
    /// <returns>The same block.</returns>
    public static BlockNode With(this BlockNode node, string name, string value) => node.Set(name, value);
}