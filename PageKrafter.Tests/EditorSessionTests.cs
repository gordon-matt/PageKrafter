namespace PageKrafter.Tests;

public class EditorSessionTests
{
    private readonly ManualTimeProvider time = new();

    [Fact]
    public void Insert_adds_the_block_and_selects_it()
    {
        var session = CreateSession(TestHost.Page());

        var result = session.Insert(BlockTypes.Heading, PageDocument.RootId, SlotDefinition.Default, 0);

        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, session.SelectedId);
        Assert.Equal(BlockTypes.Heading, Assert.Single(session.Document.Root.ChildrenOf(SlotDefinition.Default)).Type);
    }

    [Fact]
    public void Insert_rejects_blocks_that_are_not_available_in_email()
    {
        var session = CreateSession(PageDocument.Create(DocumentMode.Email));

        var result = session.Insert(BlockTypes.Video, PageDocument.RootId, SlotDefinition.Default, 0);

        Assert.False(result.IsSuccess);
        Assert.Empty(session.Document.Root.ChildrenOf(SlotDefinition.Default));
    }

    [Fact]
    public void Insert_rejects_slots_the_parent_does_not_have()
    {
        var heading = Nodes.Heading("Title");
        var session = CreateSession(TestHost.Page(heading));

        var result = session.Insert(BlockTypes.Text, heading.Id, SlotDefinition.Default, 0);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Move_within_the_same_slot_accounts_for_the_removed_block()
    {
        var first = Nodes.Heading("One");
        var second = Nodes.Heading("Two");
        var third = Nodes.Heading("Three");
        var session = CreateSession(TestHost.Page(first, second, third));

        var result = session.Move(first.Id, PageDocument.RootId, SlotDefinition.Default, 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Two", "One", "Three"], Texts(session));
    }

    [Fact]
    public void Move_refuses_to_put_a_block_inside_itself()
    {
        var inner = Nodes.Box();
        var outer = Nodes.Section(inner);
        var session = CreateSession(TestHost.Page(outer));

        var result = session.Move(outer.Id, inner.Id, SlotDefinition.Default, 0);

        Assert.False(result.IsSuccess);
        Assert.Same(outer, Assert.Single(session.Document.Root.ChildrenOf(SlotDefinition.Default)));
    }

    [Fact]
    public void Delete_selects_the_parent_block()
    {
        var heading = Nodes.Heading("Title");
        var section = Nodes.Section(heading);
        var session = CreateSession(TestHost.Page(section));
        session.Select(heading.Id);

        session.Delete(heading.Id);

        Assert.Equal(section.Id, session.SelectedId);
        Assert.Empty(section.ChildrenOf(SlotDefinition.Default));
    }

    [Fact]
    public void Duplicate_gives_the_copy_and_its_children_new_ids()
    {
        var heading = Nodes.Heading("Title");
        var section = Nodes.Section(heading);
        var session = CreateSession(TestHost.Page(section));

        var result = session.Duplicate(section.Id);

        var children = session.Document.Root.ChildrenOf(SlotDefinition.Default);
        Assert.Equal(2, children.Count);
        Assert.Equal(result.Value, children[1].Id);
        Assert.NotEqual(heading.Id, children[1].ChildrenOf(SlotDefinition.Default)[0].Id);
    }

    [Fact]
    public void Undo_and_redo_restore_the_document()
    {
        var session = CreateSession(TestHost.Page());
        session.Insert(BlockTypes.Heading, PageDocument.RootId, SlotDefinition.Default, 0);

        Assert.True(session.Undo());
        Assert.Empty(session.Document.Root.ChildrenOf(SlotDefinition.Default));

        Assert.True(session.Redo());
        Assert.Single(session.Document.Root.ChildrenOf(SlotDefinition.Default));
    }

    [Fact]
    public void Quick_edits_to_one_field_are_undone_together()
    {
        var heading = Nodes.Heading("A");
        var session = CreateSession(TestHost.Page(heading));

        session.SetProp(heading.Id, "text", "Ab");
        time.Now += TimeSpan.FromMilliseconds(500);
        session.SetProp(heading.Id, "text", "Abc");
        time.Now += TimeSpan.FromSeconds(5);
        session.SetProp(heading.Id, "text", "Abcd");

        session.Undo();
        Assert.Equal("Abc", session.Document.Root.ChildrenOf(SlotDefinition.Default)[0].Get("text"));

        session.Undo();
        Assert.Equal("A", session.Document.Root.ChildrenOf(SlotDefinition.Default)[0].Get("text"));
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void Section_templates_insert_with_fresh_ids_each_time()
    {
        var template = new PageTemplate { Kind = TemplateKind.Section, Content = Nodes.Columns("", [Nodes.Heading("A")], [Nodes.Heading("B")]) };
        var session = CreateSession(TestHost.Page());

        var first = session.InsertTemplate(template, PageDocument.RootId, SlotDefinition.Default, 0);
        var second = session.InsertTemplate(template, PageDocument.RootId, SlotDefinition.Default, 1);

        Assert.True(first.IsSuccess);
        Assert.NotEqual(first.Value, second.Value);
        Assert.NotEqual(template.Content.Id, first.Value);
    }

    [Fact]
    public void Page_templates_cannot_be_inserted_as_sections()
    {
        var template = new PageTemplate { Kind = TemplateKind.Page, Content = Nodes.Page() };
        var session = CreateSession(TestHost.Page());

        Assert.False(session.InsertTemplate(template, PageDocument.RootId, SlotDefinition.Default, 0).IsSuccess);
    }

    [Fact]
    public void Applying_a_page_template_replaces_the_page_and_can_be_undone()
    {
        var existing = Nodes.Heading("Mine");
        var session = CreateSession(TestHost.Page(existing));
        var template = new PageTemplate { Kind = TemplateKind.Page, Content = Nodes.Page(Nodes.Heading("Template")) };

        session.ApplyPageTemplate(template);
        Assert.Equal(["Template"], Texts(session));
        Assert.Equal(PageDocument.RootId, session.Document.Root.Id);

        session.Undo();
        Assert.Equal(["Mine"], Texts(session));
    }

    [Fact]
    public void Click_to_add_goes_into_a_selected_container()
    {
        var section = Nodes.Section();
        var session = CreateSession(TestHost.Page(section));
        session.Select(section.Id);

        session.InsertNearSelection(BlockTypes.Text);

        Assert.Single(section.ChildrenOf(SlotDefinition.Default));
    }

    [Fact]
    public void Click_to_add_goes_after_a_selected_leaf_block()
    {
        var heading = Nodes.Heading("One");
        var session = CreateSession(TestHost.Page(heading, Nodes.Heading("Three")));
        session.Select(heading.Id);

        string id = session.InsertNearSelection(BlockTypes.Heading).Value;
        session.SetProp(id, "text", "Two");

        Assert.Equal(["One", "Two", "Three"], Texts(session));
    }

    private EditorSession CreateSession(PageDocument document) =>
        new(TestHost.Registry(), document, timeProvider: time);

    private static string[] Texts(EditorSession session) =>
        session.Document.Root.ChildrenOf(SlotDefinition.Default).Select(node => node.Get("text") ?? string.Empty).ToArray();
}