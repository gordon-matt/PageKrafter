using Heroicsoft.NetForge.Components;
using PageKrafter.Demo;
using PageKrafter.Demo.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDemoServices(builder.Environment.ContentRootPath);

builder.Services.ConfigureNetForgeToastSounds(sounds => sounds.EnableSounds = true);

var app = builder.Build();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();