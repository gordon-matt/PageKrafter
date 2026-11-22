namespace PageKrafter.Templates;

/// <summary>
/// A read-only source of templates, such as the built-in layouts or a set shipped by a CMS module.
/// Register with <c>AddPageKrafterTemplates&lt;T&gt;()</c>.
/// </summary>
public interface ITemplateProvider
{
    /// <summary>The templates this source offers. The template service marks them read-only, so editors can use them but not change or delete them.</summary>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The templates.</returns>
    Task<IReadOnlyList<PageTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Where templates saved by editors are kept. The default keeps them in memory; use
/// <c>AddPageKrafterJsonFileTemplates</c> for files, or register your own (for example, a database table).
/// </summary>
public interface ITemplateStore
{
    /// <summary>The templates editors have saved. Read-only provider templates are not included.</summary>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The saved templates.</returns>
    Task<IReadOnlyList<PageTemplate>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>The saved template with this id.</summary>
    /// <param name="id">The template id.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The template, or <see langword="null"/> when it is not in the store.</returns>
    Task<PageTemplate?> FindAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Stores the template, replacing one that already has the same id.</summary>
    /// <param name="template">The template to store.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    /// <returns>A task that completes when the template is stored.</returns>
    Task SaveAsync(PageTemplate template, CancellationToken cancellationToken = default);

    /// <summary>Removes the saved template.</summary>
    /// <param name="id">The template id.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns><see langword="true"/> when a template was removed.</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}