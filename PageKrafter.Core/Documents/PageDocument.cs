namespace PageKrafter.Documents;

/// <summary>
/// The saved form of a page or email. This JSON is the only thing to persist; HTML is always
/// generated from it, never parsed back.
/// </summary>
public sealed class PageDocument
{
    public const int CurrentVersion = 1;
    public const string RootId = "root";

    public int Version { get; set; } = CurrentVersion;

    public DocumentMode Mode { get; set; } = DocumentMode.Web;

    public BlockNode Root { get; set; } = NewRoot();

    public static PageDocument Create(DocumentMode mode = DocumentMode.Web) => new() { Mode = mode };

    public static BlockNode NewRoot() => new() { Id = RootId, Type = BlockTypes.Page };

    public BlockNode? Find(string? id) =>
        id is null ? null : Root.DescendantsAndSelf().FirstOrDefault(node => string.Equals(node.Id, id, StringComparison.Ordinal));

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

    public PageDocument Clone() => new() { Version = Version, Mode = Mode, Root = Root.Clone() };
}
