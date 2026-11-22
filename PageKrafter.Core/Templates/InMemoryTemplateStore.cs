using System.Collections.Concurrent;

namespace PageKrafter.Templates;

/// <summary>Keeps saved templates in memory for the process. They are lost when the process stops.</summary>
public sealed class InMemoryTemplateStore : ITemplateStore
{
    private readonly ConcurrentDictionary<string, PageTemplate> templates = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public Task<IReadOnlyList<PageTemplate>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PageTemplate>>(templates.Values.ToArray());

    /// <inheritdoc/>
    public Task<PageTemplate?> FindAsync(string id, CancellationToken cancellationToken = default) =>
        Task.FromResult(templates.TryGetValue(id, out var template) ? template : null);

    /// <inheritdoc/>
    public Task SaveAsync(PageTemplate template, CancellationToken cancellationToken = default)
    {
        templates[template.Id] = template;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        Task.FromResult(templates.TryRemove(id, out _));
}