using Microsoft.Extensions.Logging;

namespace PageKrafter.Templates;

/// <summary>Lists, saves, and deletes templates. Provider templates are read-only. Saved templates go to the store.</summary>
public interface ITemplateService
{
    /// <summary>
    /// Templates for one document mode. Provider templates are marked read-only and listed after saved ones.
    /// Within that, they are ordered by category and then name, ignoring case. The mode must match exactly.
    /// </summary>
    /// <param name="mode">The document mode.</param>
    /// <param name="kind">When set, only this kind is returned.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The matching templates.</returns>
    Task<Result<IReadOnlyList<PageTemplate>>> ListAsync(DocumentMode mode, TemplateKind? kind = null, CancellationToken cancellationToken = default);

    /// <summary>The template from a provider or the store. A provider template is returned as read-only.</summary>
    /// <param name="id">The template id.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The template, or not found.</returns>
    Task<Result<PageTemplate>> FindAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a template. The name is trimmed and must be present and no longer than <see cref="TemplateService.MaxNameLength"/>.
    /// A page template must contain a page, and a section template must not. A blank category becomes <see cref="TemplateService.SavedCategory"/>.
    /// A new id starts with <c>t</c>. Replacing a read-only template is forbidden. The stored content is a copy.
    /// </summary>
    /// <param name="form">The name, kind, and mode.</param>
    /// <param name="content">The page root, or the block to insert.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>The stored template, or invalid or forbidden.</returns>
    Task<Result<PageTemplate>> SaveAsync(TemplateForm form, BlockNode content, CancellationToken cancellationToken = default);

    /// <summary>Deletes a saved template. A missing id is not found, and a provider template is forbidden.</summary>
    /// <param name="id">The template id.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>Success, not found, or forbidden.</returns>
    Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="ITemplateService"/>
public sealed class TemplateService(
    IEnumerable<ITemplateProvider> providers,
    ITemplateStore store,
    TimeProvider timeProvider,
    ILogger<TemplateService> logger) : ITemplateService
{
    /// <summary>The category used when an editor leaves the category blank. The value is "My templates".</summary>
    public const string SavedCategory = "My templates";

    /// <summary>The longest template name, in characters. The value is 100.</summary>
    public const int MaxNameLength = 100;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<PageTemplate>>> ListAsync(DocumentMode mode, TemplateKind? kind = null, CancellationToken cancellationToken = default)
    {
        var all = new List<PageTemplate>();
        foreach (var provider in providers)
        {
            all.AddRange((await provider.GetTemplatesAsync(cancellationToken)).Select(template => template with { IsReadOnly = true }));
        }

        all.AddRange(await store.ListAsync(cancellationToken));

        var matching = all
            .Where(template => template.Mode == mode && (kind is null || template.Kind == kind))
            .OrderBy(template => template.IsReadOnly ? 1 : 0)
            .ThenBy(template => template.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(template => template.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Result<IReadOnlyList<PageTemplate>>.Success(matching);
    }

    /// <inheritdoc/>
    public async Task<Result<PageTemplate>> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        var template = await FindAnyAsync(id, cancellationToken);
        return template is null ? Result<PageTemplate>.NotFound() : Result<PageTemplate>.Success(template);
    }

    /// <inheritdoc/>
    public async Task<Result<PageTemplate>> SaveAsync(TemplateForm form, BlockNode content, CancellationToken cancellationToken = default)
    {
        string name = form.Name.Trim();
        if (name.Length == 0)
        {
            return Result<PageTemplate>.Invalid(new ValidationError(nameof(form.Name), "Give the template a name."));
        }

        if (name.Length > MaxNameLength)
        {
            return Result<PageTemplate>.Invalid(new ValidationError(nameof(form.Name), $"Keep the name under {MaxNameLength} characters."));
        }

        bool isPage = content.Type == BlockTypes.Page;
        if (isPage != (form.Kind == TemplateKind.Page))
        {
            return Result<PageTemplate>.Invalid(new ValidationError(nameof(form.Kind), isPage
                ? "A whole page can only be saved as a page template."
                : "Only a whole page can be saved as a page template."));
        }

        string id = string.IsNullOrWhiteSpace(form.Id) ? "t" + NodeId.New()[1..] : form.Id;
        if (!string.IsNullOrWhiteSpace(form.Id))
        {
            var existing = await FindAnyAsync(id, cancellationToken);
            if (existing?.IsReadOnly == true)
            {
                return Result<PageTemplate>.Forbidden();
            }
        }

        var template = new PageTemplate
        {
            Id = id,
            Name = name,
            Description = form.Description.Trim(),
            Category = string.IsNullOrWhiteSpace(form.Category) ? SavedCategory : form.Category.Trim(),
            Kind = form.Kind,
            Mode = form.Mode,
            Content = content.Clone(),
            UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime,
        };

        await store.SaveAsync(template, cancellationToken);
        logger.LogInformation("Saved {Kind} template {TemplateId} named {TemplateName}.", template.Kind, template.Id, template.Name);
        return Result<PageTemplate>.Success(template);
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var template = await FindAnyAsync(id, cancellationToken);
        if (template is null)
        {
            return Result.NotFound();
        }

        if (template.IsReadOnly)
        {
            return Result.Forbidden();
        }

        await store.DeleteAsync(id, cancellationToken);
        logger.LogInformation("Deleted template {TemplateId}.", id);
        return Result.Success();
    }

    private async Task<PageTemplate?> FindAnyAsync(string id, CancellationToken cancellationToken)
    {
        foreach (var provider in providers)
        {
            var match = (await provider.GetTemplatesAsync(cancellationToken)).FirstOrDefault(template => template.Id == id);
            if (match is not null)
            {
                return match with { IsReadOnly = true };
            }
        }

        return await store.FindAsync(id, cancellationToken);
    }
}