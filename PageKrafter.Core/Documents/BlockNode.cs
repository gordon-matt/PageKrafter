namespace PageKrafter.Documents;

/// <summary>
/// One block in the page tree. Props are stored as strings so the JSON stays flat and every
/// field editor can bind to it the same way. Child blocks live in named slots.
/// </summary>
public sealed class BlockNode
{
    /// <summary>The block id. A new block receives one from <see cref="NodeId.New"/>. The comparison is case-sensitive.</summary>
    public string Id { get; set; } = NodeId.New();

    /// <summary>The block type key, such as <c>heading</c> or a host key such as <c>cms.content-zone</c>.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Field values, stored as strings. Names are matched case-sensitively.</summary>
    public Dictionary<string, string> Props { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Child blocks by slot name. Names are matched case-sensitively.</summary>
    public Dictionary<string, List<BlockNode>> Slots { get; set; } = new(StringComparer.Ordinal);

    /// <summary>The stored prop, or <see langword="null"/> when it has never been set.</summary>
    /// <param name="name">The prop name.</param>
    /// <returns>The stored text, or <see langword="null"/>.</returns>
    public string? Get(string name) => Props.TryGetValue(name, out string? value) ? value : null;

    /// <summary>Stores a prop and returns this block. A <see langword="null"/> value removes the prop.</summary>
    /// <param name="name">The prop name.</param>
    /// <param name="value">The text to store, or <see langword="null"/> to remove it.</param>
    /// <returns>This block.</returns>
    public BlockNode Set(string name, string? value)
    {
        if (value is null)
        {
            Props.Remove(name);
        }
        else
        {
            Props[name] = value;
        }

        return this;
    }

    /// <summary>The children of a slot, creating the list when the slot has not been used yet.</summary>
    /// <param name="name">The slot name.</param>
    /// <returns>The live list. Adding to it changes the tree.</returns>
    public List<BlockNode> Slot(string name)
    {
        if (!Slots.TryGetValue(name, out var children))
        {
            children = [];
            Slots[name] = children;
        }

        return children;
    }

    /// <summary>The children of a slot. A missing slot is an empty list and is not created.</summary>
    /// <param name="slot">The slot name.</param>
    /// <returns>The children, or an empty list.</returns>
    public IReadOnlyList<BlockNode> ChildrenOf(string slot) =>
        Slots.TryGetValue(slot, out var children) ? children : [];

    /// <summary>This block, then its children, depth first and left to right.</summary>
    /// <returns>The blocks.</returns>
    public IEnumerable<BlockNode> DescendantsAndSelf()
    {
        var stack = new Stack<BlockNode>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            foreach (var children in node.Slots.Values.Reverse())
            {
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    stack.Push(children[i]);
                }
            }
        }
    }

    /// <summary>A copy of this block and its children.</summary>
    /// <param name="newIds"><see langword="true"/> gives every copied block a new id. <see langword="false"/> keeps the ids.</param>
    /// <returns>The copy.</returns>
    public BlockNode Clone(bool newIds = false)
    {
        var copy = new BlockNode
        {
            Id = newIds ? NodeId.New() : Id,
            Type = Type,
            Props = new Dictionary<string, string>(Props, StringComparer.Ordinal),
        };

        foreach (var (name, children) in Slots)
        {
            copy.Slots[name] = children.Select(child => child.Clone(newIds)).ToList();
        }

        return copy;
    }
}