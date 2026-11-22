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

    /// <summary><see langword="true"/> when <see cref="Undo(string)"/> can restore an earlier snapshot.</summary>
    public bool CanUndo => undo.Count > 0;

    /// <summary><see langword="true"/> when <see cref="Redo(string)"/> can restore a snapshot that was undone.</summary>
    public bool CanRedo => redo.Count > 0;

    /// <summary>
    /// Stores the document as it was before a change. The same <paramref name="coalesceKey"/> inside <see cref="CoalesceWindow"/> does not add another step, so typing stays one undo.
    /// Recording always clears redo. A limit below 1 keeps one snapshot, and the oldest snapshot is dropped past the limit.
    /// </summary>
    /// <param name="snapshot">The document JSON from before the change.</param>
    /// <param name="coalesceKey">A key such as one field. <see langword="null"/> always starts a new step.</param>
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

    /// <summary>Restores the latest snapshot and keeps <paramref name="current"/> for redo.</summary>
    /// <param name="current">The document JSON to put on the redo stack.</param>
    /// <returns>The snapshot to restore, or <see langword="null"/> when there is nothing to undo.</returns>
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

    /// <summary>Restores the snapshot that was undone and keeps <paramref name="current"/> for undo.</summary>
    /// <param name="current">The document JSON to put on the undo stack.</param>
    /// <returns>The snapshot to restore, or <see langword="null"/> when there is nothing to redo.</returns>
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

    /// <summary>Drops the undo and redo stacks and the coalesce key.</summary>
    public void Clear()
    {
        undo.Clear();
        redo.Clear();
        lastKey = null;
    }
}