using System.Collections.Concurrent;

namespace PageKrafter.Templates;

public sealed class InMemoryTemplateStore : ITemplateStore
{
    private readonly ConcurrentDictionary<string, PageTemplate> templates = new(StringComparer.Ordinal);

    public Task<IReadOnlyList<PageTemplate>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PageTemplate>>(templates.Values.ToArray());

    public Task<PageTemplate?> FindAsync(string id, CancellationToken cancellationToken = default) =>
        Task.FromResult(templates.TryGetValue(id, out var template) ? template : null);

    public Task SaveAsync(PageTemplate template, CancellationToken cancellationToken = default)
    {
        templates[template.Id] = template;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        Task.FromResult(templates.TryRemove(id, out _));
}