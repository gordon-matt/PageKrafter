using System.Collections.Concurrent;

namespace PageKrafter.Templates;

/// <summary>
/// Represents the in memory template store.
/// </summary>
public sealed class InMemoryTemplateStore : ITemplateStore
{
    private readonly ConcurrentDictionary<string, PageTemplate> templates = new(StringComparer.Ordinal);

    /// <summary>
    /// Lists the value.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the page templates.</returns>
    public Task<IReadOnlyList<PageTemplate>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PageTemplate>>(templates.Values.ToArray());

    /// <summary>
    /// Finds the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the page template.</returns>
    public Task<PageTemplate?> FindAsync(string id, CancellationToken cancellationToken = default) =>
        Task.FromResult(templates.TryGetValue(id, out var template) ? template : null);

    /// <summary>
    /// Saves the value.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SaveAsync(PageTemplate template, CancellationToken cancellationToken = default)
    {
        templates[template.Id] = template;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true when the condition is met; otherwise, false.</returns>
    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        Task.FromResult(templates.TryRemove(id, out _));
}