using System.Text.Json;
using System.Text.RegularExpressions;

namespace PageKrafter.Templates;

/// <summary>Keeps each template as <c>{id}.json</c> in one folder, which suits small sites and source-controlled template sets.</summary>
public sealed partial class JsonFileTemplateStore(string directory) : ITemplateStore
{
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <summary>The folder that holds one <c>{id}.json</c> file per template.</summary>
    public string Directory { get; } = directory;

    /// <summary>Reads every JSON file in the folder. A missing folder, a file that is not a template, and a file that cannot be read are skipped.</summary>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The templates that could be read.</returns>
    public async Task<IReadOnlyList<PageTemplate>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (!System.IO.Directory.Exists(Directory))
        {
            return [];
        }

        var templates = new List<PageTemplate>();
        foreach (string file in System.IO.Directory.EnumerateFiles(Directory, "*.json"))
        {
            var template = await ReadAsync(file, cancellationToken);
            if (template is not null)
            {
                templates.Add(template);
            }
        }

        return templates;
    }

    /// <summary>Reads <c>{id}.json</c>. An id that is not one to 64 letters, digits, underscores, or hyphens is treated as missing.</summary>
    /// <param name="id">The template id.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The template, or <see langword="null"/> when the file is missing, unreadable, or not a template.</returns>
    public async Task<PageTemplate?> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        string? path = PathFor(id);
        return path is not null && File.Exists(path) ? await ReadAsync(path, cancellationToken) : null;
    }

    /// <summary>Writes <c>{id}.json</c> through a temporary file, creating the folder when it does not exist.</summary>
    /// <param name="template">The template to store.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    /// <returns>A task that completes when the file has been replaced.</returns>
    /// <exception cref="ArgumentException">The template id is not one to 64 letters, digits, underscores, or hyphens.</exception>
    public async Task SaveAsync(PageTemplate template, CancellationToken cancellationToken = default)
    {
        string path = PathFor(template.Id) ?? throw new ArgumentException($"\"{template.Id}\" is not a valid template id.", nameof(template));
        await gate.WaitAsync(cancellationToken);
        try
        {
            System.IO.Directory.CreateDirectory(Directory);
            string temporary = path + ".tmp";
            await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(template, PageKrafterJson.IndentedOptions), cancellationToken);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>Deletes <c>{id}.json</c>. A disallowed id or a missing file returns <see langword="false"/>.</summary>
    /// <param name="id">The template id.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns><see langword="true"/> when the file was deleted.</returns>
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        string? path = PathFor(id);
        if (path is null)
        {
            return false;
        }

        await gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            File.Delete(path);
            return true;
        }
        finally
        {
            gate.Release();
        }
    }

    private string? PathFor(string id) => IdPattern().IsMatch(id) ? Path.Combine(Directory, id + ".json") : null;

    private static async Task<PageTemplate?> ReadAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            var template = await JsonSerializer.DeserializeAsync<PageTemplate>(stream, PageKrafterJson.Options, cancellationToken);
            if (template?.Content is null)
            {
                return null;
            }

            PageKrafterJson.NormalizeNode(template.Content, new HashSet<string>(StringComparer.Ordinal));
            return template with { IsReadOnly = false };
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{1,64}$")]
    private static partial Regex IdPattern();
}