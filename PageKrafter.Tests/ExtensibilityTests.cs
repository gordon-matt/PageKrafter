using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using PageKrafter.Components;

namespace PageKrafter.Tests;

public class ExtensibilityTests
{
    [Fact]
    public void Host_blocks_replace_built_ins_whatever_the_registration_order()
    {
        var services = new ServiceCollection();
        services.AddPageKrafterBlock<CustomHeadingBlock>();
        services.AddPageKrafterCore();

        var registry = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<BlockRegistry>();

        Assert.IsType<CustomHeadingBlock>(registry.Find(BlockTypes.Heading));
    }

    [Fact]
    public void Disabled_blocks_are_removed_but_the_page_block_is_kept()
    {
        var registry = TestHost.Registry(services => services.Configure<PageKrafterOptions>(options =>
        {
            options.DisabledBlocks.Add(BlockTypes.Video);
            options.DisabledBlocks.Add(BlockTypes.Page);
        }));

        Assert.Null(registry.Find(BlockTypes.Video));
        Assert.NotNull(registry.Find(BlockTypes.Page));
    }

    [Fact]
    public void The_palette_only_offers_blocks_for_the_mode()
    {
        var registry = TestHost.Registry();

        var email = registry.PaletteFor(DocumentMode.Email).Select(definition => definition.Type).ToArray();

        Assert.Contains(BlockTypes.Columns, email);
        Assert.DoesNotContain(BlockTypes.Video, email);
        Assert.DoesNotContain(BlockTypes.Page, email);
    }

    [Fact]
    public async Task Host_blocks_without_an_element_still_get_editor_markers()
    {
        var provider = TestHost.Create(services => services.AddPageKrafterBlock<TokenBlock>());
        var block = Nodes.Create(TokenBlock.TypeName);
        var document = TestHost.Page(block);
        var renderer = provider.GetRequiredService<PageRenderer>();

        string export = await renderer.RenderAsync(document, RenderOptions.Export with { IncludeBaseStyles = false });
        string editor = await renderer.RenderAsync(document, RenderOptions.Editor);

        Assert.Contains("[[Token]]", export);
        Assert.DoesNotContain("pk-wrap", export);
        Assert.Contains($"data-pk-id=\"{block.Id}\"", editor);
    }

    [Fact]
    public async Task Host_block_errors_show_a_placeholder_in_the_editor()
    {
        var provider = TestHost.Create(services => services.AddPageKrafterBlock<BrokenBlock>());
        var document = TestHost.Page(Nodes.Create(BrokenBlock.TypeName));

        string editor = await provider.GetRequiredService<PageRenderer>().RenderAsync(document, RenderOptions.Editor);

        Assert.Contains("pk-missing", editor);
        Assert.Contains("boom", editor);
    }

    [Fact]
    public async Task Component_blocks_render_their_slots_through_a_razor_component()
    {
        var provider = TestHost.Create(services => services.AddPageKrafterBlock<PanelBlock>());
        var panel = Nodes.Create(PanelBlock.TypeName, ("title", "Note")).Add(Nodes.Heading("Inside"));
        var document = TestHost.Page(panel);
        var renderer = provider.GetRequiredService<PageRenderer>();

        string export = await renderer.RenderAsync(document, RenderOptions.Export with { IncludeBaseStyles = false });
        string editor = await renderer.RenderAsync(document, RenderOptions.Editor);

        Assert.Contains("<section class=\"panel\"><strong>Note</strong><div><h2", export);
        Assert.Contains("Inside", export);
        Assert.Contains($"data-pk-id=\"{panel.Id}\"", editor);
        Assert.Contains("data-pk-slot=\"default\"", editor);
    }

    private sealed class CustomHeadingBlock : BlockDefinition
    {
        public override string Type => BlockTypes.Heading;

        public override string Label => "Custom heading";

        public override ValueTask<string> RenderAsync(BlockRenderContext context) => ValueTask.FromResult("<h1>custom</h1>");
    }

    private sealed class TokenBlock : BlockDefinition
    {
        public const string TypeName = "test.token";

        public override string Type => TypeName;

        public override string Label => "Token";

        public override ValueTask<string> RenderAsync(BlockRenderContext context) => ValueTask.FromResult("[[Token]]");
    }

    private sealed class BrokenBlock : BlockDefinition
    {
        public const string TypeName = "test.broken";

        public override string Type => TypeName;

        public override string Label => "Broken";

        public override ValueTask<string> RenderAsync(BlockRenderContext context) => throw new InvalidOperationException("boom");
    }

    private sealed class PanelBlock : ComponentBlockDefinition<PanelView>
    {
        public const string TypeName = "test.panel";

        public override string Type => TypeName;

        public override string Label => "Panel";

        public override IReadOnlyList<BlockField> Fields { get; } = [BlockField.Text("title", "Title")];

        public override IReadOnlyList<SlotDefinition> GetSlots(BlockNode node) => [new(SlotDefinition.Default, "the panel")];
    }

    private sealed class PanelView : BlockComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "section");
            builder.AddAttribute(1, "class", "panel");
            builder.OpenElement(2, "strong");
            builder.AddContent(3, Value("title"));
            builder.CloseElement();
            builder.OpenElement(4, "div");
            builder.AddMultipleAttributes(5, SlotAttributes());
            builder.AddContent(6, Slot());
            builder.CloseElement();
            builder.CloseElement();
        }
    }
}
