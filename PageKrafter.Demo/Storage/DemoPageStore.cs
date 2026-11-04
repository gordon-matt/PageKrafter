using System.Text.RegularExpressions;

namespace PageKrafter.Demo.Storage;

/// <summary>Keeps each page's JSON in a file. A CMS would keep it in a column next to the page's HTML.</summary>
public sealed partial class DemoPageStore(string directory)
{
    public async Task<string?> LoadAsync(string name, CancellationToken cancellationToken = default)
    {
        string path = PathFor(name);
        return File.Exists(path) ? await File.ReadAllTextAsync(path, cancellationToken) : null;
    }

    public async Task SaveAsync(string name, PageDocument document, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(PathFor(name), PageKrafterJson.Serialize(document, indented: true), cancellationToken);
    }

    private string PathFor(string name) =>
        NamePattern().IsMatch(name) ? Path.Combine(directory, name + ".json") : throw new ArgumentException($"\"{name}\" is not a valid page name.", nameof(name));

    [GeneratedRegex("^[a-z0-9-]{1,40}$")]
    private static partial Regex NamePattern();
}