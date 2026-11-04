using static PageKrafter.Templates.Nodes;

namespace PageKrafter.Templates;

public sealed class BuiltInTemplateProvider : ITemplateProvider
{
    public const string LayoutCategory = "Layouts";
    public const string ContentCategory = "Content";
    public const string PagesCategory = "Starter pages";

    private const string ShortText = "<p>Add a short description here. Double-click to edit.</p>";

    private static readonly Lazy<IReadOnlyList<PageTemplate>> Templates = new(Build);

    public Task<IReadOnlyList<PageTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Templates.Value);

    private static IReadOnlyList<PageTemplate> Build() =>
    [
        Section("two-columns", "Two columns", LayoutCategory, Columns(string.Empty, ColumnContent(), ColumnContent())),
        Section("three-columns", "Three columns", LayoutCategory, Columns(string.Empty, ColumnContent(), ColumnContent(), ColumnContent())),
        Section("four-columns", "Four columns", LayoutCategory, Columns(string.Empty, ColumnContent(), ColumnContent(), ColumnContent(), ColumnContent())),
        Section("sidebar-left", "Sidebar left", LayoutCategory, Columns("1:3", ColumnContent("Sidebar"), ColumnContent("Main content"))),
        Section("sidebar-right", "Sidebar right", LayoutCategory, Columns("3:1", ColumnContent("Main content"), ColumnContent("Sidebar"))),
        Section("hero", "Hero banner", ContentCategory, Hero()),
        Section("feature-trio", "Three features", ContentCategory, Features()),
        Section("image-text", "Image and text", ContentCategory, ImageAndText()),
        Section("call-to-action", "Call to action", ContentCategory, CallToAction()),

        Page("blank", "Blank page", DocumentMode.Web, Nodes.Page()),
        Page("two-column-page", "Two-column page", DocumentMode.Web, Nodes.Page(
            Nodes.Section(Heading("Page title", "h1"), Text(ShortText)),
            Nodes.Section(Columns(string.Empty, ColumnContent(), ColumnContent())))),
        Page("three-column-page", "Three-column page", DocumentMode.Web, Nodes.Page(
            Nodes.Section(Heading("Page title", "h1"), Text(ShortText)),
            Nodes.Section(Columns(string.Empty, ColumnContent(), ColumnContent(), ColumnContent())))),
        Page("sidebar-page", "Page with sidebar", DocumentMode.Web, Nodes.Page(
            Nodes.Section(Heading("Page title", "h1")),
            Nodes.Section(Columns("3:1", ColumnContent("Main content"), ColumnContent("Sidebar"))))),
        Page("landing-page", "Landing page", DocumentMode.Web, Nodes.Page(Hero(), Features(), ImageAndText(), CallToAction())),

        EmailSection("email-two-columns", "Two columns", LayoutCategory, EmailRow(Columns(string.Empty, ColumnContent(), ColumnContent()))),
        EmailSection("email-hero", "Hero image", ContentCategory, EmailRow(Image(), Spacer("16px"), Heading("A headline for your email", "h1"), Text(ShortText), Button("Read more"))),
        EmailSection("email-button", "Button row", ContentCategory, EmailRow(Button("Call to action"))),
        EmailSection("email-footer", "Footer", ContentCategory, EmailFooter()),

        Page("email-blank", "Blank email", DocumentMode.Email, Nodes.Page()),
        Page("email-newsletter", "Newsletter", DocumentMode.Email, Nodes.Page(
            EmailRow(Heading("Your Company", "h3").With(CommonStyles.Align, "center")),
            EmailRow(Image(), Spacer("16px"), Heading("This month's news", "h1"), Text("<p>Say hello and set the scene for this issue. Double-click to edit.</p>"), Button("Read the full story")),
            EmailRow(Columns(string.Empty, ColumnContent("First story"), ColumnContent("Second story"))),
            EmailFooter())),
    ];

    private static BlockNode[] ColumnContent(string heading = "Column heading") => [Heading(heading, "h3"), Text(ShortText)];

    private static BlockNode Hero() =>
        Nodes.Section(
                Heading("A bold headline that explains your offer", "h1").With(CommonStyles.Color, "#ffffff"),
                Text("<p>One or two sentences that support the headline and tell visitors what to do next.</p>"),
                Button("Get started").With("size", "lg"))
            .With(CommonStyles.Padding, "96px 16px")
            .With(CommonStyles.Background, "#0f172a")
            .With(CommonStyles.Color, "#ffffff")
            .With(CommonStyles.Align, "center")
            .With("maxWidth", "760px");

    private static BlockNode Features() =>
        Nodes.Section(
            Heading("Why people choose us").With(CommonStyles.Align, "center"),
            Spacer("16px"),
            Columns(string.Empty, Feature("Fast"), Feature("Reliable"), Feature("Friendly")));

    private static BlockNode[] Feature(string title) => [Image(), Heading(title, "h3"), Text(ShortText)];

    private static BlockNode ImageAndText() =>
        Nodes.Section(Columns(
                string.Empty,
                [Image()],
                [Heading("Tell your story"), Text("<p>Explain what makes you different. Keep it short and specific.</p>"), Button("Find out more")]))
            .With(CommonStyles.Background, "#f8fafc");

    private static BlockNode CallToAction() =>
        Nodes.Section(Columns(
                "2:1",
                [Heading("Ready to start?").With(CommonStyles.Color, "#ffffff"), Text("<p>A final nudge for visitors who scrolled this far.</p>")],
                [Button("Contact us").With(CommonStyles.Align, "right")]))
            .With(CommonStyles.Background, "#2563eb")
            .With(CommonStyles.Color, "#ffffff")
            .With("verticalAlign", "center");

    private static BlockNode EmailRow(params BlockNode[] children) =>
        Nodes.Section(children).With(CommonStyles.Padding, "24px");

    private static BlockNode EmailFooter() =>
        EmailRow(Text("<p>You're receiving this because you signed up on our website.<br><a href=\"#\">Unsubscribe</a></p>"))
            .With(CommonStyles.Align, "center")
            .With(CommonStyles.Color, "#6b7280");

    private static PageTemplate Section(string id, string name, string category, BlockNode content) =>
        new() { Id = $"builtin-{id}", Name = name, Category = category, Kind = TemplateKind.Section, Mode = DocumentMode.Web, Content = content, IsReadOnly = true };

    private static PageTemplate EmailSection(string id, string name, string category, BlockNode content) =>
        new() { Id = $"builtin-{id}", Name = name, Category = category, Kind = TemplateKind.Section, Mode = DocumentMode.Email, Content = content, IsReadOnly = true };

    private static PageTemplate Page(string id, string name, DocumentMode mode, BlockNode content) =>
        new() { Id = $"builtin-{id}", Name = name, Category = PagesCategory, Kind = TemplateKind.Page, Mode = mode, Content = content, IsReadOnly = true };
}