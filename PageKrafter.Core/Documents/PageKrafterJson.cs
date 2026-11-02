using System.Text.Json;
using System.Text.Json.Serialization;

namespace PageKrafter.Documents;

public static class PageKrafterJson
{
    public static JsonSerializerOptions Options { get; } = Create(indented: false);

    public static JsonSerializerOptions IndentedOptions { get; } = Create(indented: true);

    public static string Serialize(PageDocument document, bool indented = false) =>
        JsonSerializer.Serialize(document, indented ? IndentedOptions : Options);

    /// <summary>An empty or blank string gives a new document so hosts can bind a brand new field.</summary>
    public static Result<PageDocument> Deserialize(string? json, DocumentMode fallbackMode = DocumentMode.Web)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Result<PageDocument>.Success(PageDocument.Create(fallbackMode));
        }

        try
        {
            var document = JsonSerializer.Deserialize<PageDocument>(json, Options);
            if (document is null)
            {
                return Result<PageDocument>.Invalid(new ValidationError("The page data is empty."));
            }

            Normalize(document);
            return Result<PageDocument>.Success(document);
        }
        catch (JsonException ex)
        {
            return Result<PageDocument>.Invalid(new ValidationError($"The page data isn't valid JSON: {ex.Message}"));
        }
    }

    /// <summary>
    /// Repairs data written by hand or by older versions: missing collections, null values,
    /// a missing root and duplicate ids (which would break selection in the canvas).
    /// </summary>
    public static void Normalize(PageDocument document)
    {
        document.Root ??= PageDocument.NewRoot();
        document.Root.Type = BlockTypes.Page;
        if (string.IsNullOrWhiteSpace(document.Root.Id))
        {
            document.Root.Id = PageDocument.RootId;
        }

        if (document.Mode is not (DocumentMode.Web or DocumentMode.Email))
        {
            document.Mode = DocumentMode.Web;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        NormalizeNode(document.Root, seen);
    }

    internal static void NormalizeNode(BlockNode node, HashSet<string> seen)
    {
        node.Type ??= string.Empty;
        node.Props ??= new Dictionary<string, string>(StringComparer.Ordinal);
        node.Slots ??= new Dictionary<string, List<BlockNode>>(StringComparer.Ordinal);

        foreach (string key in node.Props.Where(pair => pair.Value is null).Select(pair => pair.Key).ToList())
        {
            node.Props.Remove(key);
        }

        if (string.IsNullOrWhiteSpace(node.Id) || !seen.Add(node.Id))
        {
            node.Id = NodeId.New();
            seen.Add(node.Id);
        }

        foreach (string slot in node.Slots.Keys.ToList())
        {
            var children = node.Slots[slot] ?? [];
            children.RemoveAll(child => child is null);
            node.Slots[slot] = children;
            foreach (var child in children)
            {
                NormalizeNode(child, seen);
            }
        }
    }

    private static JsonSerializerOptions Create(bool indented)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = indented,
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
