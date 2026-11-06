namespace PageKrafter.Tests;

public class PageRendererTests
{
    [Fact]
    public async Task Export_has_no_editor_attributes()
    {
        string html = await RenderAsync(TestHost.Page(Nodes.Heading("Hello")), RenderOptions.Export);

        Assert.Contains("<h2", html);
        Assert.Contains("Hello", html);
        Assert.DoesNotContain("data-pk-", html);
    }

    [Fact]
    public async Task Editor_marks_blocks_slots_and_inline_fields()
    {
        var heading = Nodes.Heading("Hello");
        string html = await RenderAsync(TestHost.Page(Nodes.Section(heading)), RenderOptions.Editor);

        Assert.Contains($"data-pk-id=\"{heading.Id}\"", html);
        Assert.Contains("data-pk-slot=\"default\"", html);
        Assert.Contains("data-pk-edit=\"text\"", html);
        Assert.Contains("data-pk-root", html);
    }

    [Fact]
    public async Task Common_styles_merge_into_the_block_element()
    {
        var heading = Nodes.Heading("Hello")
            .With(CommonStyles.Padding, "12px")
            .With(CommonStyles.CssClass, "lead")
            .With(CommonStyles.Anchor, "intro");

        string html = await RenderAsync(TestHost.Page(heading), RenderOptions.Export);

        Assert.Contains("padding:12px", html);
        Assert.Contains("lead", html);
        Assert.Contains("id=\"intro\"", html);
        Assert.Equal(1, Count(html, "<h2"));
    }

    [Fact]
    public async Task Style_values_cannot_break_out_of_the_declaration()
    {
        var heading = Nodes.Heading("Hello").With(CommonStyles.Background, "red;}</style><script>alert(1)</script>");

        string html = await RenderAsync(TestHost.Page(heading), RenderOptions.Export);

        Assert.DoesNotContain("<script", html);
    }

    [Fact]
    public async Task Rich_text_is_sanitized()
    {
        var text = Nodes.Text("<p onclick=\"steal()\">Hi<script>alert(1)</script></p>");

        string html = await RenderAsync(TestHost.Page(text), RenderOptions.Export);

        Assert.Contains("Hi", html);
        Assert.DoesNotContain("<script", html);
        Assert.DoesNotContain("onclick", html);
    }

    [Fact]
    public async Task Unsafe_links_are_dropped()
    {
        string html = await RenderAsync(TestHost.Page(Nodes.Button("Click", "javascript:alert(1)")), RenderOptions.Export);

        Assert.DoesNotContain("javascript:", html);
    }

    [Fact]
    public async Task Unknown_blocks_are_kept_quietly_in_export_and_flagged_in_the_editor()
    {
        var document = TestHost.Page(Nodes.Create("acme.missing"));

        Assert.Equal("<div class=\"pk-page\"></div>", await RenderAsync(document, RenderOptions.Export));
        Assert.Contains("Unknown block", await RenderAsync(document, RenderOptions.Editor));
    }

    [Fact]
    public async Task Email_columns_render_as_tables()
    {
        var document = new PageDocument
        {
            Mode = DocumentMode.Email,
            Root = Nodes.Page(Nodes.Columns("1:2", [Nodes.Text("<p>A</p>")], [Nodes.Text("<p>B</p>")])),
        };

        string html = await RenderAsync(document, RenderOptions.Export);

        Assert.Contains("role=\"presentation\"", html);
        Assert.Contains("width=\"33.33%\"", html);
        Assert.Contains("width=\"66.67%\"", html);
        Assert.DoesNotContain("display:flex", html);
    }

    [Fact]
    public async Task Web_columns_use_the_width_ratios()
    {
        string html = await RenderAsync(TestHost.Page(Nodes.Columns("1:3", [], [])), RenderOptions.Export);

        Assert.Contains("flex:1 1 0%", html);
        Assert.Contains("flex:3 3 0%", html);
    }

    [Fact]
    public async Task Visibility_adds_a_responsive_class_on_the_web_only()
    {
        var heading = Nodes.Heading("Desktop only").With(CommonStyles.Hide, "mobile");
        var email = new PageDocument { Mode = DocumentMode.Email, Root = Nodes.Page(heading.Clone()) };

        Assert.Contains("pk-hide-mobile", await RenderAsync(TestHost.Page(heading), RenderOptions.Export));
        Assert.DoesNotContain("pk-hide-mobile", await RenderAsync(email, RenderOptions.Export));
    }

    [Fact]
    public async Task A_full_document_includes_the_base_styles()
    {
        var renderer = TestHost.Create().GetRequiredService<PageRenderer>();

        string html = await renderer.RenderDocumentAsync(TestHost.Page(Nodes.Heading("Hi")), "Title & more");

        Assert.StartsWith("<!DOCTYPE html>", html);
        Assert.Contains("<title>Title &amp; more</title>", html);
        Assert.Contains(".pk-columns", html);
    }

    [Fact]
    public async Task Editor_marks_token_fields_and_leaves_other_text_alone()
    {
        string heading = await RenderAsync(TestHost.Page(Nodes.Heading("Hello")), RenderOptions.Editor);
        string button = await RenderAsync(TestHost.Page(Nodes.Button("Go")), RenderOptions.Editor);

        Assert.Contains("data-pk-token=\"\"", heading);
        Assert.DoesNotContain("data-pk-token", button);
    }

    [Fact]
    public void Text_heading_and_html_accept_tokens()
    {
        var registry = TestHost.Registry();

        Assert.Contains(registry.Find(BlockTypes.Heading)!.Fields, field => field.Name == "text" && field.AcceptsTokens);
        Assert.Contains(registry.Find(BlockTypes.Text)!.Fields, field => field.Name == "content" && field.AcceptsTokens);
        Assert.Contains(registry.Find(BlockTypes.Html)!.Fields, field => field.Name == "html" && field.AcceptsTokens);
        Assert.DoesNotContain(registry.Find(BlockTypes.Button)!.Fields, field => field.AcceptsTokens);
    }

    private static Task<string> RenderAsync(PageDocument document, RenderOptions options) =>
        TestHost.Create().GetRequiredService<PageRenderer>().RenderAsync(document, options with { IncludeBaseStyles = false });

    private static int Count(string text, string value) => text.Split(value).Length - 1;
}