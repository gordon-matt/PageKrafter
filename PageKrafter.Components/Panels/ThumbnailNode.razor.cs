namespace PageKrafter.Components.Panels;

/// <summary>
/// Represents the thumbnail node.
/// </summary>
public partial class ThumbnailNode
{
    private const int MaxDepth = 4;
    private const int MaxChildren = 5;

    /// <summary>
    /// Gets or sets the node.
    /// </summary>
    [Parameter, EditorRequired]
    public BlockNode Node { get; set; } = default!;

    /// <summary>
    /// Gets or sets the depth.
    /// </summary>
    [Parameter]
    public int Depth { get; set; }

    private IReadOnlyList<BlockNode> Children => Node.Slots.Values.SelectMany(children => children).ToArray();

    private IEnumerable<(string Ratio, IReadOnlyList<BlockNode> Children)> Columns
    {
        get
        {
            int count = ColumnsBlock.CountOf(Node);
            decimal[] ratios = ColumnsBlock.ParseRatios(Node.Get("widths"), count);
            for (int i = 0; i < count; i++)
            {
                yield return (ratios[i].ToString(System.Globalization.CultureInfo.InvariantCulture), Node.ChildrenOf(ColumnsBlock.SlotName(i)));
            }
        }
    }

    private string Align => Node.Get(CommonStyles.Align) switch
    {
        "center" => "margin-left:auto;margin-right:auto",
        "right" => "margin-left:auto",
        _ => string.Empty,
    };

    private string Band => Css.Declarations(("background-color", Node.Get(CommonStyles.Background)));
}