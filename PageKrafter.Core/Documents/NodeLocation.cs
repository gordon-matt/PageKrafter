namespace PageKrafter.Documents;

/// <summary>Where a block sits in its parent. The page root has no location, because nothing contains it.</summary>
/// <param name="Parent">The block that contains this one.</param>
/// <param name="Slot">The slot name on <paramref name="Parent"/>.</param>
/// <param name="Index">The position in that slot, starting at zero.</param>
public sealed record NodeLocation(BlockNode Parent, string Slot, int Index)
{
    /// <summary>The block at <see cref="Index"/>.</summary>
    public BlockNode Node => Parent.Slots[Slot][Index];
}