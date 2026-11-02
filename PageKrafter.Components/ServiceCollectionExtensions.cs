using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PageKrafter.Components;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the editor and everything from <c>AddPageKrafterCore</c>. Add host blocks with
        /// <c>AddPageKrafterBlock&lt;T&gt;()</c> and a template store with <c>AddPageKrafterJsonFileTemplates</c>
        /// or <c>AddPageKrafterTemplateStore&lt;T&gt;()</c>.
        /// </summary>
        public IServiceCollection AddPageKrafter(Action<PageKrafterOptions>? configure = null)
        {
            services.AddPageKrafterCore(configure);
            services.TryAddScoped<BlockComponentRenderer>();
            return services;
        }

        /// <summary>Uses a Razor component for every field whose editor is <paramref name="editor"/>. Use a built-in name, such as <c>image</c>, to replace that editor.</summary>
        public IServiceCollection AddPageKrafterFieldEditor<TComponent>(string editor)
            where TComponent : FieldEditorBase =>
            services.Configure<PageKrafterOptions>(options => options.FieldEditorComponents[editor] = typeof(TComponent));
    }
}
