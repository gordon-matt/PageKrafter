using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PageKrafter;

/// <summary>Registers rendering, the built-in blocks, and template storage without the Blazor editor.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the block registry, renderer and template services without the Blazor editor.
        /// Use this where pages are only rendered, such as a public site. The editor's <c>AddPageKrafter</c> calls it.
        /// A sanitizer, template store, registry, renderer, or file-root provider the host already registered is left in place.
        /// Register a file-root provider before this call so each tenant can open its own folder.
        /// </summary>
        /// <param name="configure">Adjusts the options. <see langword="null"/> keeps the defaults.</param>
        /// <returns>The same service collection.</returns>
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
            services.TryAddScoped<IFileRootProvider, DefaultFileRootProvider>();
            return services;
        }

        /// <summary>
        /// Adds a block type. Built-in definitions are applied first, so a host block that reuses a built-in type name replaces that built-in.
        /// A later registration of the same type name replaces the earlier one. A type listed in <see cref="PageKrafterOptions.DisabledBlocks"/> is omitted, except the page block.
        /// </summary>
        /// <typeparam name="TBlock">The block type. It is created per scope, so it can take scoped services.</typeparam>
        /// <returns>The same service collection.</returns>
        public IServiceCollection AddPageKrafterBlock<TBlock>()
            where TBlock : BlockDefinition
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped<BlockDefinition, TBlock>());
            return services;
        }

        /// <summary>Adds a read-only template source. Every registered provider is listed. A second registration of the same type is ignored.</summary>
        /// <typeparam name="TProvider">The provider type. It is created per scope.</typeparam>
        /// <returns>The same service collection.</returns>
        public IServiceCollection AddPageKrafterTemplates<TProvider>()
            where TProvider : class, ITemplateProvider
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped<ITemplateProvider, TProvider>());
            return services;
        }

        /// <summary>Replaces where saved templates are kept. The store is scoped so it can use a database context. The default store keeps templates in memory for the process.</summary>
        /// <typeparam name="TStore">The store type.</typeparam>
        /// <returns>The same service collection.</returns>
        public IServiceCollection AddPageKrafterTemplateStore<TStore>()
            where TStore : class, ITemplateStore
        {
            services.Replace(ServiceDescriptor.Scoped<ITemplateStore, TStore>());
            return services;
        }

        /// <summary>
        /// Replaces the template store with one JSON file per template in <paramref name="directory"/>.
        /// The folder is created on the first save. An id that is not one to 64 letters, digits, underscores, or hyphens cannot be found, saved, or deleted.
        /// </summary>
        /// <param name="directory">The folder that holds the template files.</param>
        /// <returns>The same service collection.</returns>
        public IServiceCollection AddPageKrafterJsonFileTemplates(string directory)
        {
            services.Replace(ServiceDescriptor.Singleton<ITemplateStore>(new JsonFileTemplateStore(directory)));
            return services;
        }
    }
}