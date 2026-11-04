namespace PageKrafter.Templates;

/// <summary>
/// A read-only source of templates, such as the built-in layouts or a set shipped by a CMS module.
/// Register with <c>AddPageKrafterTemplates&lt;T&gt;()</c>.
/// </summary>
public interface ITemplateProvider
{
    Task<IReadOnlyList<PageTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Where templates saved by editors are kept. The default keeps them in memory; use
/// <c>AddPageKrafterJsonFileTemplates</c> for files, or register your own (for example, a database table).
/// </summary>
public interface ITemplateStore
{
    Task<IReadOnlyList<PageTemplate>> ListAsync(CancellationToken cancellationToken = default);

    Task<PageTemplate?> FindAsync(string id, CancellationToken cancellationToken = default);

    Task SaveAsync(PageTemplate template, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}