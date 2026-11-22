namespace PageKrafter.Documents;

/// <summary>
/// The saved form of a page or email. This JSON is the only thing to persist; HTML is always
/// generated from it, never parsed back.
/// </summary>
public sealed class PageDocument
{
    /// <summary>The document version written into new JSON. The value is 1.</summary>
    public const int CurrentVersion = 1;

    /// <summary>The id of the page block. The value is <c>root</c>.</summary>
    public const string RootId = "root";

    /// <summary>The document version. New documents use <see cref="CurrentVersion"/>.</summary>
    public int Version { get; set; } = CurrentVersion;

    /// <summary>Web or email. A value that is neither is read back as web.</summary>
    public DocumentMode Mode { get; set; } = DocumentMode.Web;

    /// <summary>The page block. Its children are the sections and other top-level blocks.</summary>
    public BlockNode Root { get; set; } = NewRoot();

    /// <summary>A new document with an empty page block.</summary>
    /// <param name="mode">Web or email. The default is web.</param>
    /// <returns>The document.</returns>
    public static PageDocument Create(DocumentMode mode = DocumentMode.Web) => new() { Mode = mode };

    /// <summary>An empty page block whose id is <see cref="RootId"/>.</summary>
    /// <returns>The page block.</returns>
    public static BlockNode NewRoot() => new() { Id = RootId, Type = BlockTypes.Page };

    /// <summary>The block with this id, including the root. The comparison is case-sensitive.</summary>
    /// <param name="id">The block id. <see langword="null"/> matches nothing.</param>
    /// <returns>The block, or <see langword="null"/> when no block has that id.</returns>
    public BlockNode? Find(string? id) =>
        id is null ? null : Root.DescendantsAndSelf().FirstOrDefault(node => string.Equals(node.Id, id, StringComparison.Ordinal));

    /// <summary>The parent, slot, and index of a block. The root has no location.</summary>
    /// <param name="id">The block id. <see langword="null"/> matches nothing.</param>
    /// <returns>The location, or <see langword="null"/> for the root or an unknown id.</returns>
    public NodeLocation? Locate(string? id)
    {
        if (id is null)
        {
            return null;
        }

        foreach (var parent in Root.DescendantsAndSelf())
        {
            foreach (var (slot, children) in parent.Slots)
            {
                int index = children.FindIndex(child => string.Equals(child.Id, id, StringComparison.Ordinal));
                if (index >= 0)
                {
                    return new NodeLocation(parent, slot, index);
                }
            }
        }

        return null;
    }

    /// <summary>A copy of the document, including the tree. Ids are kept.</summary>
    /// <returns>The copy.</returns>
    public PageDocument Clone() => new() { Version = Version, Mode = Mode, Root = Root.Clone() };
}