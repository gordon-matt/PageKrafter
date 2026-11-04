namespace PageKrafter.Editing;

/// <summary>
/// Undo and redo as whole-document JSON snapshots. Pages are small, so snapshots are simpler and
/// far more robust than replaying individual operations.
/// </summary>
public sealed class EditHistory(int limit = 100, TimeProvider? timeProvider = null)
{
    /// <summary>Edits to the same field within this window become one undo step, so typing a heading isn't undone a letter at a time.</summary>
    public static readonly TimeSpan CoalesceWindow = TimeSpan.FromSeconds(1.5);

    private readonly LinkedList<string> undo = new();
    private readonly Stack<string> redo = new();
    private readonly TimeProvider time = timeProvider ?? TimeProvider.System;
    private string? lastKey;
    private DateTimeOffset lastAt;

    public bool CanUndo => undo.Count > 0;

    public bool CanRedo => redo.Count > 0;

    public void Record(string snapshot, string? coalesceKey = null)
    {
        var now = time.GetUtcNow();
        bool merge = coalesceKey is not null
            && string.Equals(coalesceKey, lastKey, StringComparison.Ordinal)
            && now - lastAt < CoalesceWindow
            && undo.Count > 0;

        lastKey = coalesceKey;
        lastAt = now;
        redo.Clear();
        if (merge)
        {
            return;
        }

        undo.AddLast(snapshot);
        if (undo.Count > Math.Max(1, limit))
        {
            undo.RemoveFirst();
        }
    }

    public string? Undo(string current)
    {
        if (undo.Last is not { } last)
        {
            return null;
        }

        undo.RemoveLast();
        redo.Push(current);
        lastKey = null;
        return last.Value;
    }

    public string? Redo(string current)
    {
        if (!redo.TryPop(out string? next))
        {
            return null;
        }

        undo.AddLast(current);
        lastKey = null;
        return next;
    }

    public void Clear()
    {
        undo.Clear();
        redo.Clear();
        lastKey = null;
    }
}