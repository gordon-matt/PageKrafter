namespace PageKrafter.Editing;

/// <summary>
/// The editing state behind one editor: the document, the selection and undo history. Every change
/// goes through here so nesting rules and history are applied the same way whether the change
/// came from the canvas, the layers panel or the inspector.
/// </summary>
public sealed class EditorSession
{
    private readonly BlockRegistry registry;
    private readonly EditHistory history;
    private readonly Func<BlockDefinition, bool>? filter;

    /// <summary>Starts a session for one document. The history is empty and nothing is selected.</summary>
    /// <param name="registry">The blocks the session can insert.</param>
    /// <param name="document">The document to edit.</param>
    /// <param name="historyLimit">How many undo snapshots to keep. The default is 100, and a value below 1 keeps one.</param>
    /// <param name="filter">An extra palette filter. <see langword="null"/> allows every block the registry offers for the document mode, except the page block.</param>
    /// <param name="timeProvider">The clock for the typing window. <see langword="null"/> uses the system clock.</param>
    public EditorSession(
        BlockRegistry registry,
        PageDocument document,
        int historyLimit = 100,
        Func<BlockDefinition, bool>? filter = null,
        TimeProvider? timeProvider = null)
    {
        this.registry = registry;
        this.filter = filter;
        history = new EditHistory(historyLimit, timeProvider);
        Document = document;
    }

    /// <summary>The document being edited. <see cref="Load"/> replaces it.</summary>
    public PageDocument Document { get; private set; }

    /// <summary>The selected block id, or <see langword="null"/> when nothing is selected.</summary>
    public string? SelectedId { get; private set; }

    /// <summary>The selected block, or <see langword="null"/> when nothing is selected or the id is gone.</summary>
    public BlockNode? Selected => Document.Find(SelectedId);

    /// <summary><see langword="true"/> when undo can restore an earlier document.</summary>
    public bool CanUndo => history.CanUndo;

    /// <summary><see langword="true"/> when redo can restore a document that was undone.</summary>
    public bool CanRedo => history.CanRedo;

    /// <summary>Whether the block can be added: it is not the page block, it includes the document mode, and the session filter allows it.</summary>
    /// <param name="definition">The block.</param>
    /// <returns><see langword="true"/> when it can be inserted.</returns>
    public bool IsAllowed(BlockDefinition definition) =>
        definition.Type != BlockTypes.Page
        && definition.Modes.HasFlag(Document.Mode)
        && (filter?.Invoke(definition) ?? true);

    /// <summary>Replaces the document, clears the selection, and drops the undo history.</summary>
    /// <param name="document">The document to edit.</param>
    public void Load(PageDocument document)
    {
        Document = document;
        SelectedId = null;
        history.Clear();
    }

    /// <summary>Selects the block when it exists. An unknown id, or <see langword="null"/>, clears the selection.</summary>
    /// <param name="id">The block id.</param>
    public void Select(string? id) => SelectedId = Document.Find(id) is not null ? id : null;

    /// <summary>Creates a block and inserts it. An unknown or disallowed type is invalid with "That block isn't available here."</summary>
    /// <param name="blockType">The block type key.</param>
    /// <param name="parentId">The block that receives it.</param>
    /// <param name="slot">The slot name.</param>
    /// <param name="index">The position in the slot. A value past the end is clamped.</param>
    /// <returns>The new block id, or invalid when the block cannot go there.</returns>
    public Result<string> Insert(string blockType, string parentId, string slot, int index)
    {
        var definition = registry.Find(blockType);
        return definition is null || !IsAllowed(definition)
            ? Invalid<string>("That block isn't available here.")
            : InsertNode(definition.Create(), parentId, slot, index);
    }

    /// <summary>
    /// Inserts a section template, with new ids. A page template is invalid and says to apply it instead.
    /// A template for the other document mode is invalid.
    /// </summary>
    /// <param name="template">The section template.</param>
    /// <param name="parentId">The block that receives it.</param>
    /// <param name="slot">The slot name.</param>
    /// <param name="index">The position in the slot. A value past the end is clamped.</param>
    /// <returns>The inserted block id, or invalid.</returns>
    public Result<string> InsertTemplate(PageTemplate template, string parentId, string slot, int index)
    {
        string? error = TemplateError(template);
        return error is null ? InsertNode(template.Instantiate(), parentId, slot, index) : Invalid<string>(error);
    }

    /// <summary>Click-to-add: into the selected block when it has a slot that can hold the new block, otherwise after it, otherwise at the end of the page.</summary>
    /// <param name="blockType">The block type key.</param>
    /// <returns>The new block id, or invalid when the type is unknown or disallowed.</returns>
    public Result<string> InsertNearSelection(string blockType)
    {
        var definition = registry.Find(blockType);
        return definition is null || !IsAllowed(definition)
            ? Invalid<string>("That block isn't available here.")
            : InsertNear(definition.Create());
    }

    /// <summary>Inserts a section template in the same place as <see cref="InsertNearSelection"/>. A page template or the wrong mode is invalid.</summary>
    /// <param name="template">The section template.</param>
    /// <returns>The inserted block id, or invalid.</returns>
    public Result<string> InsertTemplateNearSelection(PageTemplate template)
    {
        string? error = TemplateError(template);
        return error is null ? InsertNear(template.Instantiate()) : Invalid<string>(error);
    }

    /// <summary>
    /// Moves a block. The page root cannot be moved, and a block cannot be moved inside itself.
    /// Moving it later in the same slot accounts for the hole it leaves. Dropping it where it already is changes nothing and records no history.
    /// </summary>
    /// <param name="id">The block to move.</param>
    /// <param name="parentId">The block that receives it.</param>
    /// <param name="slot">The slot name.</param>
    /// <param name="index">The position in the slot.</param>
    /// <returns>Success, or invalid when the block cannot go there.</returns>
    public Result Move(string id, string parentId, string slot, int index)
    {
        var location = Document.Locate(id);
        if (location is null)
        {
            return Result.Invalid(new ValidationError("That block can't be moved."));
        }

        var node = location.Node;
        if (node.DescendantsAndSelf().Any(candidate => candidate.Id == parentId))
        {
            return Result.Invalid(new ValidationError("A block can't be moved inside itself."));
        }

        string? error = PlacementError(node, parentId, slot);
        if (error is not null)
        {
            return Result.Invalid(new ValidationError(error));
        }

        var parent = Document.Find(parentId)!;
        bool sameList = ReferenceEquals(parent, location.Parent) && location.Slot == slot;
        int target = sameList && location.Index < index ? index - 1 : index;
        if (sameList && target == location.Index)
        {
            return Result.Success();
        }

        Commit(() =>
        {
            location.Parent.Slots[location.Slot].RemoveAt(location.Index);
            var children = parent.Slot(slot);
            children.Insert(Math.Clamp(target, 0, children.Count), node);
        });

        SelectedId = id;
        return Result.Success();
    }

    /// <summary>Moves a block among its siblings. An offset that does not change its place records no history. The page root cannot be moved.</summary>
    /// <param name="id">The block to move.</param>
    /// <param name="offset">How many places to move. Negative moves it earlier.</param>
    /// <returns>Success, or invalid when the block cannot be moved.</returns>
    public Result MoveBy(string id, int offset)
    {
        var location = Document.Locate(id);
        if (location is null)
        {
            return Result.Invalid(new ValidationError("That block can't be moved."));
        }

        var children = location.Parent.Slots[location.Slot];
        int target = Math.Clamp(location.Index + offset, 0, children.Count - 1);
        if (target == location.Index)
        {
            return Result.Success();
        }

        Commit(() =>
        {
            var node = children[location.Index];
            children.RemoveAt(location.Index);
            children.Insert(target, node);
        });

        return Result.Success();
    }

    /// <summary>
    /// Removes a block. The page root cannot be deleted.
    /// If the deleted block was selected, the selection moves to its parent, or clears when that parent is the page.
    /// </summary>
    /// <param name="id">The block to remove.</param>
    /// <returns>Success, or invalid for the page root.</returns>
    public Result Delete(string id)
    {
        var location = Document.Locate(id);
        if (location is null)
        {
            return Result.Invalid(new ValidationError("The page itself can't be deleted."));
        }

        Commit(() => location.Parent.Slots[location.Slot].RemoveAt(location.Index));
        if (SelectedId is not null && Document.Find(SelectedId) is null)
        {
            SelectedId = ReferenceEquals(location.Parent, Document.Root) ? null : location.Parent.Id;
        }

        return Result.Success();
    }

    /// <summary>Inserts a copy, with new ids, immediately after the block and selects the copy. The page root cannot be duplicated.</summary>
    /// <param name="id">The block to copy.</param>
    /// <returns>The copy's id, or invalid for the page root.</returns>
    public Result<string> Duplicate(string id)
    {
        var location = Document.Locate(id);
        if (location is null)
        {
            return Invalid<string>("The page itself can't be duplicated.");
        }

        var copy = location.Node.Clone(newIds: true);
        Commit(() => location.Parent.Slots[location.Slot].Insert(location.Index + 1, copy));
        SelectedId = copy.Id;
        return Result<string>.Success(copy.Id);
    }

    /// <summary>
    /// Sets a prop and calls the block's property-changed hook. The same value records no history.
    /// Edits to the same prop inside the typing window become one undo step.
    /// </summary>
    /// <param name="id">The block id.</param>
    /// <param name="name">The prop name.</param>
    /// <param name="value">The text to store.</param>
    /// <returns>Success, or not found when the block is gone.</returns>
    public Result SetProp(string id, string name, string value)
    {
        var node = Document.Find(id);
        if (node is null)
        {
            return Result.NotFound();
        }

        if (string.Equals(node.Get(name), value, StringComparison.Ordinal))
        {
            return Result.Success();
        }

        Commit(() =>
        {
            node.Props[name] = value;
            registry.Find(node.Type)?.OnPropertyChanged(node, name);
        }, $"prop:{id}:{name}");

        return Result.Success();
    }

    /// <summary>
    /// Replaces the whole document with a page template and clears the selection. The change can be undone.
    /// A section template is invalid, and so is a template for the other document mode.
    /// </summary>
    /// <param name="template">The page template.</param>
    /// <returns>Success, or invalid.</returns>
    public Result ApplyPageTemplate(PageTemplate template)
    {
        if (template.Kind != TemplateKind.Page)
        {
            return Result.Invalid(new ValidationError("That is a section template. Drag it onto the page instead."));
        }

        if (template.Mode != Document.Mode)
        {
            return Result.Invalid(new ValidationError($"That template is for {Describe(template.Mode)}."));
        }

        Commit(() => Document.Root = template.Instantiate());
        SelectedId = null;
        return Result.Success();
    }

    /// <summary>Restores the previous document. The selection is cleared when that block is gone.</summary>
    /// <returns><see langword="true"/> when a snapshot was restored.</returns>
    public bool Undo() => Restore(history.Undo(Snapshot()));

    /// <summary>Restores the document that was undone. The selection is cleared when that block is gone.</summary>
    /// <returns><see langword="true"/> when a snapshot was restored.</returns>
    public bool Redo() => Restore(history.Redo(Snapshot()));

    /// <summary>The id of the block that contains this one.</summary>
    /// <param name="id">The block id.</param>
    /// <returns>The parent id, or <see langword="null"/> for the page root or an unknown id.</returns>
    public string? ParentId(string id) => Document.Locate(id)?.Parent.Id;

    private Result<string> InsertNear(BlockNode node)
    {
        var selected = Selected;
        if (selected is not null && !ReferenceEquals(selected, Document.Root))
        {
            var selectedDefinition = registry.Find(selected.Type);
            var childDefinition = registry.Find(node.Type);
            var slot = selectedDefinition?.GetSlots(selected)
                .FirstOrDefault(candidate => childDefinition is null || selectedDefinition.CanContain(selected, candidate.Name, childDefinition));

            if (slot is not null)
            {
                return InsertNode(node, selected.Id, slot.Name, selected.ChildrenOf(slot.Name).Count);
            }

            var location = Document.Locate(selected.Id)!;
            return InsertNode(node, location.Parent.Id, location.Slot, location.Index + 1);
        }

        return InsertNode(node, Document.Root.Id, SlotDefinition.Default, Document.Root.ChildrenOf(SlotDefinition.Default).Count);
    }

    private Result<string> InsertNode(BlockNode node, string parentId, string slot, int index)
    {
        string? error = PlacementError(node, parentId, slot);
        if (error is not null)
        {
            return Invalid<string>(error);
        }

        var children = Document.Find(parentId)!.Slot(slot);
        Commit(() => children.Insert(Math.Clamp(index, 0, children.Count), node));
        SelectedId = node.Id;
        return Result<string>.Success(node.Id);
    }

    private string? PlacementError(BlockNode node, string parentId, string slot)
    {
        var parent = Document.Find(parentId);
        if (parent is null)
        {
            return "The place you dropped on no longer exists.";
        }

        var parentDefinition = registry.Find(parent.Type);
        if (parentDefinition is null)
        {
            return "Blocks can't be added to an unknown block.";
        }

        var childDefinition = registry.Find(node.Type);
        return childDefinition is null
            ? parentDefinition.GetSlots(parent).Any(candidate => candidate.Name == slot) ? null : "Blocks can't go there."
            : parentDefinition.CanContain(parent, slot, childDefinition) && childDefinition.CanBePlacedIn(parent, slot)
            ? null
            : $"{childDefinition.Label} can't go inside {parentDefinition.Label}.";
    }

    private string? TemplateError(PageTemplate template) => template.Kind != TemplateKind.Section
            ? "Page templates replace the whole page. Use Apply instead."
            : template.Mode == Document.Mode ? null : $"That template is for {Describe(template.Mode)}.";

    private void Commit(Action change, string? coalesceKey = null)
    {
        history.Record(Snapshot(), coalesceKey);
        change();
    }

    private string Snapshot() => PageKrafterJson.Serialize(Document);

    private bool Restore(string? snapshot)
    {
        if (snapshot is null)
        {
            return false;
        }

        var restored = PageKrafterJson.Deserialize(snapshot, Document.Mode);
        if (!restored.IsSuccess)
        {
            return false;
        }

        Document = restored.Value;
        if (Document.Find(SelectedId) is null)
        {
            SelectedId = null;
        }

        return true;
    }

    private static string Describe(DocumentMode mode) => mode == DocumentMode.Email ? "emails" : "web pages";

    private static Result<T> Invalid<T>(string message) => Result<T>.Invalid(new ValidationError(message));
}