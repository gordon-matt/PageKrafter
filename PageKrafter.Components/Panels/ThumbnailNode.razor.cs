namespace PageKrafter.Components.Panels;

/// <summary>One block in a template preview. The drawing stops after four levels and five children, and columns keep their width ratio.</summary>
public partial class ThumbnailNode
{
    private const int MaxDepth = 4;
    private const int MaxChildren = 5;

    /// <summary>The block to draw.</summary>
    [Parameter, EditorRequired]
    public BlockNode Node { get; set; } = default!;

    /// <summary>How deep this block is. Drawing stops at four.</summary>
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