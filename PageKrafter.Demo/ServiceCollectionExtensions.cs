using PageKrafter.Demo.Storage;

namespace PageKrafter.Demo;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDemoServices(string contentRoot)
        {
            string data = Path.Combine(contentRoot, "App_Data");

            services.AddPageKrafter(options => options.CanvasStyles.Add("css/site-content.css"));

            services.AddPageKrafterBlock<ContentZoneBlock>();
            services.AddPageKrafterBlock<CalloutBlock>();
            services.AddPageKrafterFieldEditor<CalloutTonePicker>(CalloutBlock.ToneEditor);
            services.AddPageKrafterJsFilePicker("DemoFiles.pick");
            services.AddPageKrafterJsonFileTemplates(Path.Combine(data, "templates"));

            services.AddSingleton<IZoneCatalog, DemoZoneCatalog>();
            services.AddSingleton(new DemoPageStore(Path.Combine(data, "pages")));
            return services;
        }
    }
}