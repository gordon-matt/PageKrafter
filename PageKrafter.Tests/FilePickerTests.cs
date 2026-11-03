using Microsoft.JSInterop;
using PageKrafter.Components;

namespace PageKrafter.Tests;

public class FilePickerTests
{
    [Fact]
    public async Task The_js_picker_calls_the_function_with_the_request_and_returns_the_chosen_file()
    {
        var js = new FakeJs(new PickedFile("/media/photo.png", "photo.png"));
        var picker = new JsFilePicker(js, "App.pickFile");

        var picked = await picker.PickAsync(new FilePickRequest(FilePickerKind.Image, "Image", "/media/old.png"));

        Assert.Equal(new PickedFile("/media/photo.png", "photo.png"), picked);
        Assert.Equal("App.pickFile", js.Identifier);
        var argument = Assert.IsAssignableFrom<IDictionary<string, object?>>(Assert.Single(js.Arguments!));
        Assert.Equal("image", argument["kind"]);
        Assert.Equal("Image", argument["title"]);
        Assert.Equal("/media/old.png", argument["current"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task The_js_picker_treats_a_closed_file_manager_as_no_choice(string? url)
    {
        var js = new FakeJs(url is null ? null : new PickedFile(url));

        Assert.Null(await new JsFilePicker(js, "App.pickFile").PickAsync(new FilePickRequest(FilePickerKind.Any)));
    }

    [Fact]
    public void Pickers_are_registered_for_the_editor_and_a_later_registration_wins()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IJSRuntime>(new FakeJs(null));
        services.AddPageKrafterJsFilePicker("App.pickFile");
        services.AddPageKrafterFilePicker<StubPicker>();

        using var scope = services.BuildServiceProvider().CreateScope();

        Assert.IsType<StubPicker>(scope.ServiceProvider.GetRequiredService<IFilePicker>());
    }

    [Fact]
    public void No_picker_is_registered_unless_the_host_adds_one()
    {
        var services = new ServiceCollection();
        services.AddPageKrafter();

        using var scope = services.BuildServiceProvider().CreateScope();

        Assert.Null(scope.ServiceProvider.GetService<IFilePicker>());
    }

    [Fact]
    public void Image_and_video_fields_ask_for_the_right_kind_of_file()
    {
        var registry = TestHost.Registry();

        Assert.Equal(FieldEditors.Image, registry.Find(BlockTypes.Image)!.Fields.Single(field => field.Name == "src").Editor);
        Assert.Equal(FieldEditors.Image, registry.Find(BlockTypes.Section)!.Fields.Single(field => field.Name == "backgroundImage").Editor);
        Assert.Equal(FilePickerKind.Video, registry.Find(BlockTypes.Video)!.Fields.Single(field => field.Name == "url").FilePicker);
    }

    private sealed class StubPicker : IFilePicker
    {
        public ValueTask<PickedFile?> PickAsync(FilePickRequest request, CancellationToken cancellationToken = default) => ValueTask.FromResult<PickedFile?>(null);
    }

    private sealed class FakeJs(PickedFile? result) : IJSRuntime
    {
        public string? Identifier { get; private set; }

        public object?[]? Arguments { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Identifier = identifier;
            Arguments = args;
            return ValueTask.FromResult((TValue)(object?)result!);
        }
    }
}
