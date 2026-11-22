using Heroicsoft.NetForge.Components.Toasts;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PageKrafter.Components;

/// <summary>Registers the Blazor editor, the canvas renderer, and the core page services.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the editor and everything from <c>AddPageKrafterCore</c>. Add host blocks with
        /// <c>AddPageKrafterBlock&lt;T&gt;()</c> and a template store with <c>AddPageKrafterJsonFileTemplates</c>
        /// or <c>AddPageKrafterTemplateStore&lt;T&gt;()</c>. The editor reports through the NetForge toast service, so the host layout needs a <c>Toaster</c>.
        /// </summary>
        /// <param name="configure">Adjusts the options. <see langword="null"/> keeps the defaults.</param>
        /// <returns>The same service collection.</returns>
        public IServiceCollection AddPageKrafter(Action<PageKrafterOptions>? configure = null)
        {
            services.AddPageKrafterCore(configure);
            services.TryAddScoped<BlockComponentRenderer>();
            services.TryAddScoped<ToastService>();
            return services;
        }

        /// <summary>Uses a Razor component for every field whose editor is <paramref name="editor"/>. Use a built-in name, such as <c>image</c>, to replace that editor.</summary>
        /// <typeparam name="TComponent">The field editor. It is created by the inspector.</typeparam>
        /// <param name="editor">The editor name stored on <see cref="BlockField.Editor"/>.</param>
        /// <returns>The same service collection.</returns>
        public IServiceCollection AddPageKrafterFieldEditor<TComponent>(string editor)
            where TComponent : FieldEditorBase =>
            services.Configure<PageKrafterOptions>(options => options.FieldEditorComponents[editor] = typeof(TComponent));

        /// <summary>
        /// Plugs in a file manager. Image fields, and fields with <see cref="BlockField.FilePicker"/> set, get a Browse
        /// button that calls it. The picker is scoped to the user's circuit, so it can use other scoped services.
        /// </summary>
        /// <typeparam name="TPicker">The picker. It replaces any picker already registered.</typeparam>
        /// <returns>The same service collection.</returns>
        public IServiceCollection AddPageKrafterFilePicker<TPicker>()
            where TPicker : class, IFilePicker
        {
            services.Replace(ServiceDescriptor.Scoped<IFilePicker, TPicker>());
            return services;
        }

        /// <summary>
        /// Plugs in a browser-based file manager through a global JavaScript function, such as <c>"MyApp.pickFile"</c>.
        /// See <see cref="JsFilePicker"/> for the contract. It replaces any picker already registered.
        /// </summary>
        /// <param name="function">The global function name, such as <c>MyApp.pickFile</c>.</param>
        /// <returns>The same service collection.</returns>
        /// <exception cref="ArgumentException"><paramref name="function"/> is null or white space.</exception>
        public IServiceCollection AddPageKrafterJsFilePicker(string function)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(function);
            services.Replace(ServiceDescriptor.Scoped<IFilePicker>(provider => new JsFilePicker(provider.GetRequiredService<IJSRuntime>(), function)));
            return services;
        }
    }
}