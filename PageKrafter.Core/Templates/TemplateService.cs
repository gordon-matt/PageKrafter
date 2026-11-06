using Microsoft.Extensions.Logging;

namespace PageKrafter.Templates;

/// <summary>
/// Defines the contract for the template service.
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Lists the value.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the page templates.</returns>
    Task<Result<IReadOnlyList<PageTemplate>>> ListAsync(DocumentMode mode, TemplateKind? kind = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the page template.</returns>
    Task<Result<PageTemplate>> FindAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the value.
    /// </summary>
    /// <param name="form">The form.</param>
    /// <param name="content">The content.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the page template.</returns>
    Task<Result<PageTemplate>> SaveAsync(TemplateForm form, BlockNode content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result.</returns>
    Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the template service.
/// </summary>
public sealed class TemplateService(
    IEnumerable<ITemplateProvider> providers,
    ITemplateStore store,
    TimeProvider timeProvider,
    ILogger<TemplateService> logger) : ITemplateService
{
    /// <summary>
    /// The saved category constant.
    /// </summary>
    public const string SavedCategory = "My templates";
    /// <summary>
    /// The max name length constant.
    /// </summary>
    public const int MaxNameLength = 100;

    /// <summary>
    /// Lists the value.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the page templates.</returns>
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

    /// <summary>
    /// Finds the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the page template.</returns>
    public async Task<Result<PageTemplate>> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        var template = await FindAnyAsync(id, cancellationToken);
        return template is null ? Result<PageTemplate>.NotFound() : Result<PageTemplate>.Success(template);
    }

    /// <summary>
    /// Saves the value.
    /// </summary>
    /// <param name="form">The form.</param>
    /// <param name="content">The content.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the page template.</returns>
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

    /// <summary>
    /// Deletes the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result.</returns>
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