namespace PageKrafter.Documents;

/// <summary>
/// One block in the page tree. Props are stored as strings so the JSON stays flat and every
/// field editor can bind to it the same way. Child blocks live in named slots.
/// </summary>
public sealed class BlockNode
{
    public string Id { get; set; } = NodeId.New();

    public string Type { get; set; } = string.Empty;

    public Dictionary<string, string> Props { get; set; } = new(StringComparer.Ordinal);

    public Dictionary<string, List<BlockNode>> Slots { get; set; } = new(StringComparer.Ordinal);

    public string? Get(string name) => Props.TryGetValue(name, out string? value) ? value : null;

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

    public List<BlockNode> Slot(string name)
    {
        if (!Slots.TryGetValue(name, out var children))
        {
            children = [];
            Slots[name] = children;
        }

        return children;
    }

    public IReadOnlyList<BlockNode> ChildrenOf(string slot) =>
        Slots.TryGetValue(slot, out var children) ? children : [];

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