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

    public PageDocument Document { get; private set; }

    public string? SelectedId { get; private set; }

    public BlockNode? Selected => Document.Find(SelectedId);

    public bool CanUndo => history.CanUndo;

    public bool CanRedo => history.CanRedo;

    public bool IsAllowed(BlockDefinition definition) =>
        definition.Type != BlockTypes.Page
        && definition.Modes.HasFlag(Document.Mode)
        && (filter?.Invoke(definition) ?? true);

    public void Load(PageDocument document)
    {
        Document = document;
        SelectedId = null;
        history.Clear();
    }

    public void Select(string? id) => SelectedId = Document.Find(id) is not null ? id : null;

    public Result<string> Insert(string blockType, string parentId, string slot, int index)
    {
        var definition = registry.Find(blockType);
        if (definition is null || !IsAllowed(definition))
        {
            return Invalid<string>("That block isn't available here.");
        }

        return InsertNode(definition.Create(), parentId, slot, index);
    }

    public Result<string> InsertTemplate(PageTemplate template, string parentId, string slot, int index)
    {
        string? error = TemplateError(template);
        return error is null ? InsertNode(template.Instantiate(), parentId, slot, index) : Invalid<string>(error);
    }

    /// <summary>Click-to-add: into the selected block when it has a slot, otherwise after it, otherwise at the end of the page.</summary>
    public Result<string> InsertNearSelection(string blockType)
    {
        var definition = registry.Find(blockType);
        if (definition is null || !IsAllowed(definition))
        {
            return Invalid<string>("That block isn't available here.");
        }

        return InsertNear(definition.Create());
    }

    public Result<string> InsertTemplateNearSelection(PageTemplate template)
    {
        string? error = TemplateError(template);
        return error is null ? InsertNear(template.Instantiate()) : Invalid<string>(error);
    }

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

    public bool Undo() => Restore(history.Undo(Snapshot()));

    public bool Redo() => Restore(history.Redo(Snapshot()));

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
        if (childDefinition is null)
        {
            return parentDefinition.GetSlots(parent).Any(candidate => candidate.Name == slot) ? null : "Blocks can't go there.";
        }

        return parentDefinition.CanContain(parent, slot, childDefinition) && childDefinition.CanBePlacedIn(parent, slot)
            ? null
            : $"{childDefinition.Label} can't go inside {parentDefinition.Label}.";
    }

    private string? TemplateError(PageTemplate template)
    {
        if (template.Kind != TemplateKind.Section)
        {
            return "Page templates replace the whole page. Use Apply instead.";
        }

        return template.Mode == Document.Mode ? null : $"That template is for {Describe(template.Mode)}.";
    }

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
