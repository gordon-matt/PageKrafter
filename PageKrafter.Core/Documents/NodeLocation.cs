namespace PageKrafter.Documents;

/// <summary>
/// Represents the node location.
/// </summary>
/// <param name="Parent">The parent.</param>
/// <param name="Slot">The slot.</param>
/// <param name="Index">The index.</param>
public sealed record NodeLocation(BlockNode Parent, string Slot, int Index)
{
    /// <summary>
    /// Gets the node.
    /// </summary>
    public BlockNode Node => Parent.Slots[Slot][Index];
}