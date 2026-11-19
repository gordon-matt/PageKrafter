using Heroicsoft.NetForge.Components;
using PageKrafter.Demo;
using PageKrafter.Demo.Components;
using Pickstack;
using Pickstack.FileManager.WebFilez;
using Pickstack.RichTextEditor.TinyMce8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDemoServices(builder.Environment.ContentRootPath);

// The WebFilez browser is an MVC page shown in the picker's frame.
builder.Services.AddControllersWithViews();
builder.Services.AddPickstack(builder.Configuration)
    .AddWebFilez(files => files.UseHostBlazor())
    .Pickstack
    .AddTinyMce8(editor => editor
        .WithHeight(480)
        .ShowMenubar()
        .ShowPromotion(false)
        .WithPlugins(
            "advlist", "anchor", "autolink", "charmap", "code", "codesample", "directionality",
            "fullscreen", "help", "image", "insertdatetime", "link", "lists", "media",
            "preview", "searchreplace", "table", "visualblocks", "visualchars", "wordcount")
        .WithToolbar("undo redo | blocks fontsize | bold italic underline strikethrough | forecolor backcolor removeformat | alignleft aligncenter alignright | bullist numlist outdent indent | link image media table | code fullscreen")
        .Set("toolbar_mode", "wrap"));

builder.Services.ConfigureNetForgeToastSounds(sounds => sounds.EnableSounds = true);

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapNetForgeAssets();
app.UsePickstack();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();