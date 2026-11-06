namespace PageKrafter.Templates;

/// <summary>
/// A read-only source of templates, such as the built-in layouts or a set shipped by a CMS module.
/// Register with <c>AddPageKrafterTemplates&lt;T&gt;()</c>.
/// </summary>
public interface ITemplateProvider
{
    /// <summary>
    /// Gets the templates.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the page templates.</returns>
    Task<IReadOnlyList<PageTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Where templates saved by editors are kept. The default keeps them in memory; use
/// <c>AddPageKrafterJsonFileTemplates</c> for files, or register your own (for example, a database table).
/// </summary>
public interface ITemplateStore
{
    /// <summary>
    /// Lists the value.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the page templates.</returns>
    Task<IReadOnlyList<PageTemplate>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the page template.</returns>
    Task<PageTemplate?> FindAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the value.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveAsync(PageTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true when the condition is met; otherwise, false.</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}