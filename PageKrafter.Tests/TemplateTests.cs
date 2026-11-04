using Microsoft.Extensions.Logging.Abstractions;

namespace PageKrafter.Tests;

public sealed class TemplateTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "pagekrafter-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Built_in_templates_include_column_layouts_for_web_and_email()
    {
        var service = CreateService(new InMemoryTemplateStore());

        var web = (await service.ListAsync(DocumentMode.Web, TemplateKind.Section)).Value;
        var email = (await service.ListAsync(DocumentMode.Email)).Value;

        Assert.Contains(web, template => template.Name.Contains("Two columns", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(web, template => template.Name.Contains("Three columns", StringComparison.OrdinalIgnoreCase));
        Assert.All(web, template => Assert.True(template.IsReadOnly));
        Assert.NotEmpty(email);
        Assert.All(email, template => Assert.Equal(DocumentMode.Email, template.Mode));
    }

    [Fact]
    public async Task Saved_templates_are_listed_first_and_can_be_deleted()
    {
        var service = CreateService(new InMemoryTemplateStore());
        var form = new TemplateForm { Name = "  Promo row ", Kind = TemplateKind.Section };

        var saved = await service.SaveAsync(form, Nodes.Columns("", [Nodes.Heading("A")], [Nodes.Heading("B")]));

        Assert.True(saved.IsSuccess);
        Assert.Equal("Promo row", saved.Value.Name);
        Assert.Equal(TemplateService.SavedCategory, saved.Value.Category);
        Assert.Equal(saved.Value.Id, (await service.ListAsync(DocumentMode.Web)).Value[0].Id);

        Assert.True((await service.DeleteAsync(saved.Value.Id)).IsSuccess);
        Assert.Equal(Ardalis.Result.ResultStatus.NotFound, (await service.FindAsync(saved.Value.Id)).Status);
    }

    [Fact]
    public async Task Built_in_templates_cannot_be_overwritten_or_deleted()
    {
        var service = CreateService(new InMemoryTemplateStore());
        string id = (await service.ListAsync(DocumentMode.Web)).Value[0].Id;

        var overwrite = await service.SaveAsync(new TemplateForm { Id = id, Name = "Mine", Kind = TemplateKind.Section }, Nodes.Section());
        var delete = await service.DeleteAsync(id);

        Assert.Equal(Ardalis.Result.ResultStatus.Forbidden, overwrite.Status);
        Assert.Equal(Ardalis.Result.ResultStatus.Forbidden, delete.Status);
    }

    [Theory]
    [InlineData("", TemplateKind.Section, false)]
    [InlineData("Whole page", TemplateKind.Section, true)]
    [InlineData("Just a section", TemplateKind.Page, false)]
    public async Task Saving_validates_the_name_and_kind(string name, TemplateKind kind, bool contentIsPage)
    {
        var service = CreateService(new InMemoryTemplateStore());

        var result = await service.SaveAsync(new TemplateForm { Name = name, Kind = kind }, contentIsPage ? Nodes.Page() : Nodes.Section());

        Assert.Equal(Ardalis.Result.ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Json_file_store_persists_templates()
    {
        var saved = await CreateService(new JsonFileTemplateStore(directory))
            .SaveAsync(new TemplateForm { Name = "Hero", Kind = TemplateKind.Section }, Nodes.Section(Nodes.Heading("Big")));

        var reloaded = await new JsonFileTemplateStore(directory).FindAsync(saved.Value.Id);

        Assert.NotNull(reloaded);
        Assert.Equal("Hero", reloaded.Name);
        Assert.Equal("Big", reloaded.Content.ChildrenOf(SlotDefinition.Default)[0].Get("text"));
    }

    [Fact]
    public async Task Json_file_store_rejects_ids_that_could_escape_its_folder()
    {
        var store = new JsonFileTemplateStore(directory);

        Assert.Null(await store.FindAsync("../secrets"));
        await Assert.ThrowsAsync<ArgumentException>(() => store.SaveAsync(new PageTemplate { Id = "../x", Name = "x" }));
    }

    [Fact]
    public void Instantiating_a_page_template_keeps_the_root_id()
    {
        var template = new PageTemplate { Kind = TemplateKind.Page, Content = Nodes.Page(Nodes.Heading("A")) };

        var root = template.Instantiate();

        Assert.Equal(PageDocument.RootId, root.Id);
        Assert.NotEqual(template.Content.ChildrenOf(SlotDefinition.Default)[0].Id, root.ChildrenOf(SlotDefinition.Default)[0].Id);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static TemplateService CreateService(ITemplateStore store) =>
        new([new BuiltInTemplateProvider()], store, TimeProvider.System, NullLogger<TemplateService>.Instance);
}