namespace PageKrafter.Documents;

/// <summary>
/// The saved form of a page or email. This JSON is the only thing to persist; HTML is always
/// generated from it, never parsed back.
/// </summary>
public sealed class PageDocument
{
    /// <summary>
    /// The current version constant.
    /// </summary>
    public const int CurrentVersion = 1;
    /// <summary>
    /// The root identifier constant.
    /// </summary>
    public const string RootId = "root";

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public int Version { get; set; } = CurrentVersion;

    /// <summary>
    /// Gets or sets the mode.
    /// </summary>
    public DocumentMode Mode { get; set; } = DocumentMode.Web;

    /// <summary>
    /// Gets or sets the root.
    /// </summary>
    public BlockNode Root { get; set; } = NewRoot();

    /// <summary>
    /// Creates the value.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <returns>The page document.</returns>
    public static PageDocument Create(DocumentMode mode = DocumentMode.Web) => new() { Mode = mode };

    /// <summary>
    /// Gets the new root.
    /// </summary>
    /// <returns>The new root.</returns>
    public static BlockNode NewRoot() => new() { Id = RootId, Type = BlockTypes.Page };

    /// <summary>
    /// Finds the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The block node.</returns>
    public BlockNode? Find(string? id) =>
        id is null ? null : Root.DescendantsAndSelf().FirstOrDefault(node => string.Equals(node.Id, id, StringComparison.Ordinal));

    /// <summary>
    /// Locates the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The node location.</returns>
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

    /// <summary>
    /// Clones the value.
    /// </summary>
    /// <returns>The page document.</returns>
    public PageDocument Clone() => new() { Version = Version, Mode = Mode, Root = Root.Clone() };
}