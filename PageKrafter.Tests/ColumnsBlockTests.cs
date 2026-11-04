namespace PageKrafter.Tests;

public class ColumnsBlockTests
{
    private readonly ColumnsBlock block = new();

    [Fact]
    public void Reducing_the_count_moves_blocks_into_the_last_column()
    {
        var node = Nodes.Columns("1:1:1", [Nodes.Heading("A")], [Nodes.Heading("B")], [Nodes.Heading("C")]);

        node.Set("count", "2");
        block.OnPropertyChanged(node, "count");

        Assert.Equal(["col-1", "col-2"], node.Slots.Keys.Order());
        Assert.Equal(["B", "C"], node.ChildrenOf("col-2").Select(child => child.Get("text")));
        Assert.Null(node.Get("widths"));
    }

    [Fact]
    public void Increasing_the_count_adds_empty_columns()
    {
        var node = Nodes.Columns("", [], []);

        node.Set("count", "4");
        block.OnPropertyChanged(node, "count");

        Assert.Equal(4, block.GetSlots(node).Count);
        Assert.True(node.Slots.ContainsKey("col-4"));
    }

    [Theory]
    [InlineData("1:2", 2, new[] { 1.0, 2.0 })]
    [InlineData("1:2", 3, new[] { 1.0, 1.0, 1.0 })]
    [InlineData("0:1", 2, new[] { 1.0, 1.0 })]
    [InlineData(null, 2, new[] { 1.0, 1.0 })]
    public void Ratios_fall_back_to_equal_widths(string? widths, int count, double[] expected) => Assert.Equal(expected.Select(value => (decimal)value), ColumnsBlock.ParseRatios(widths, count));

    [Fact]
    public async Task Width_presets_follow_the_column_count()
    {
        var field = block.Fields.Single(candidate => candidate.Name == "widths");

        var options = await block.GetOptionsAsync(field, Nodes.Columns("", [], [], []));

        Assert.Contains(options, option => option.Value == "1:2:1");
        Assert.DoesNotContain(options, option => option.Value == "1:2");
    }
}