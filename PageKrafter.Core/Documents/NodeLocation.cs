namespace PageKrafter.Documents;

public sealed record NodeLocation(BlockNode Parent, string Slot, int Index)
{
    public BlockNode Node => Parent.Slots[Slot][Index];
}