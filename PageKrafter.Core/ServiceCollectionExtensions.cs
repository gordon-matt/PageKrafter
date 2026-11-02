using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PageKrafter;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the block registry, renderer and template services without the Blazor editor.
        /// Use this where pages are only rendered, such as a public site. The editor's <c>AddPageKrafter</c> calls it.
        /// </summary>
        public IServiceCollection AddPageKrafterCore(Action<PageKrafterOptions>? configure = null)
        {
            services.AddOptions<PageKrafterOptions>();
            if (configure is not null)
            {
                services.Configure(configure);
            }

            services.AddPageKrafterBlock<PageBlock>();
            services.AddPageKrafterBlock<SectionBlock>();
            services.AddPageKrafterBlock<ColumnsBlock>();
            services.AddPageKrafterBlock<ContainerBlock>();
            services.AddPageKrafterBlock<HeadingBlock>();
            services.AddPageKrafterBlock<TextBlock>();
            services.AddPageKrafterBlock<ImageBlock>();
            services.AddPageKrafterBlock<ButtonBlock>();
            services.AddPageKrafterBlock<DividerBlock>();
            services.AddPageKrafterBlock<SpacerBlock>();
            services.AddPageKrafterBlock<VideoBlock>();
            services.AddPageKrafterBlock<HtmlBlock>();

            services.TryAddSingleton(TimeProvider.System);
            services.TryAddSingleton<IHtmlContentSanitizer, HtmlContentSanitizer>();
            services.TryAddSingleton<ITemplateStore, InMemoryTemplateStore>();
            services.AddPageKrafterTemplates<BuiltInTemplateProvider>();
            services.TryAddScoped<BlockRegistry>();
            services.TryAddScoped<PageRenderer>();
            services.TryAddScoped<ITemplateService, TemplateService>();
            return services;
        }

        /// <summary>Adds a block type. Registering a type name that a built-in uses replaces the built-in.</summary>
        public IServiceCollection AddPageKrafterBlock<TBlock>()
            where TBlock : BlockDefinition
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped<BlockDefinition, TBlock>());
            return services;
        }

        public IServiceCollection AddPageKrafterTemplates<TProvider>()
            where TProvider : class, ITemplateProvider
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped<ITemplateProvider, TProvider>());
            return services;
        }

        /// <summary>Replaces where saved templates are kept. The store is scoped so it can use a DbContext.</summary>
        public IServiceCollection AddPageKrafterTemplateStore<TStore>()
            where TStore : class, ITemplateStore
        {
            services.Replace(ServiceDescriptor.Scoped<ITemplateStore, TStore>());
            return services;
        }

        public IServiceCollection AddPageKrafterJsonFileTemplates(string directory)
        {
            services.Replace(ServiceDescriptor.Singleton<ITemplateStore>(new JsonFileTemplateStore(directory)));
            return services;
        }
    }
}
