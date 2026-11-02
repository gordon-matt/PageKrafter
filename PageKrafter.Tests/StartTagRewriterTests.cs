namespace PageKrafter.Tests;

public class StartTagRewriterTests
{
    [Fact]
    public void Merges_class_and_style_with_existing_values()
    {
        string? html = StartTagRewriter.Apply("<div class=\"a\" style=\"color:red;\">x</div>", "b", "padding:4px", []);

        Assert.Equal("<div class=\"a b\" style=\"color:red;padding:4px\">x</div>", html);
    }

    [Fact]
    public void Skips_leading_whitespace_and_comments()
    {
        string? html = StartTagRewriter.Apply("  <!-- note --><p>x</p>", string.Empty, string.Empty, [("data-pk-id", "n1")]);

        Assert.Equal("  <!-- note --><p data-pk-id=\"n1\">x</p>", html);
    }

    [Fact]
    public void Keeps_self_closing_tags_and_boolean_attributes()
    {
        string? html = StartTagRewriter.Apply("<input disabled />", "c", string.Empty, []);

        Assert.Equal("<input disabled class=\"c\" />", html);
    }

    [Fact]
    public void Ignores_a_greater_than_sign_inside_a_quoted_value()
    {
        string? html = StartTagRewriter.Apply("<a title=\"1 > 0\">x</a>", "c", string.Empty, []);

        Assert.Equal("<a title=\"1 > 0\" class=\"c\">x</a>", html);
    }

    [Fact]
    public void Encodes_added_values()
    {
        string? html = StartTagRewriter.Apply("<div></div>", string.Empty, string.Empty, [("data-pk-label", "\"><script>")]);

        Assert.Equal("<div data-pk-label=\"&quot;&gt;&lt;script&gt;\"></div>", html);
    }

    [Theory]
    [InlineData("")]
    [InlineData("plain text")]
    [InlineData("[[ContentZone:Main]]")]
    public void Returns_null_when_there_is_no_leading_element(string input)
    {
        Assert.Null(StartTagRewriter.Apply(input, "c", string.Empty, []));
    }
}
