using System.Text.Json;
using System.Text.RegularExpressions;

namespace PageKrafter.Templates;

/// <summary>Keeps each template as <c>{id}.json</c> in one folder, which suits small sites and source-controlled template sets.</summary>
public sealed partial class JsonFileTemplateStore(string directory) : ITemplateStore
{
    private readonly SemaphoreSlim gate = new(1, 1);

    public string Directory { get; } = directory;

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

    public async Task<PageTemplate?> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        string? path = PathFor(id);
        return path is not null && File.Exists(path) ? await ReadAsync(path, cancellationToken) : null;
    }

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
