namespace PageKrafter.Tests;

public class DocumentTests
{
    [Fact]
    public void Round_trips_through_json()
    {
        var document = TestHost.Page(Nodes.Section(Nodes.Heading("Hello")));

        var result = PageKrafterJson.Deserialize(PageKrafterJson.Serialize(document));

        Assert.True(result.IsSuccess);
        Assert.Equal(PageKrafterJson.Serialize(document), PageKrafterJson.Serialize(result.Value));
    }

    [Fact]
    public void Blank_json_gives_a_new_document_in_the_requested_mode()
    {
        var result = PageKrafterJson.Deserialize("  ", DocumentMode.Email);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentMode.Email, result.Value.Mode);
        Assert.Equal(PageDocument.RootId, result.Value.Root.Id);
    }

    [Fact]
    public void Invalid_json_is_reported_rather_than_thrown()
    {
        var result = PageKrafterJson.Deserialize("{ not json");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Normalizing_repairs_duplicate_ids_and_nulls()
    {
        const string json = """
            {"mode":"Web","root":{"id":"root","type":"page","props":{"a":null},"slots":{"default":[
              {"id":"x","type":"heading"},{"id":"x","type":"heading"},null]}}}
            """;

        var document = PageKrafterJson.Deserialize(json).Value;
        var children = document.Root.ChildrenOf(SlotDefinition.Default);

        Assert.Equal(2, children.Count);
        Assert.NotEqual(children[0].Id, children[1].Id);
        Assert.Empty(document.Root.Props);
    }

    [Fact]
    public void Locate_finds_the_parent_slot_and_index()
    {
        var heading = Nodes.Heading("B");
        var columns = Nodes.Columns("", [Nodes.Heading("A")], [heading]);
        var document = TestHost.Page(columns);

        var location = document.Locate(heading.Id);

        Assert.NotNull(location);
        Assert.Same(columns, location.Parent);
        Assert.Equal("col-2", location.Slot);
        Assert.Equal(0, location.Index);
    }
}