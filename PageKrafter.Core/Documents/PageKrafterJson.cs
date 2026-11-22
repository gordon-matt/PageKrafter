using System.Text.Json;
using System.Text.Json.Serialization;

namespace PageKrafter.Documents;

/// <summary>Reads and writes a <see cref="PageDocument"/>. Enums are stored as names. Web defaults are used, so property names are camel case.</summary>
public static class PageKrafterJson
{
    /// <summary>Serializer options for stored JSON. The output is one line.</summary>
    public static JsonSerializerOptions Options { get; } = Create(indented: false);

    /// <summary>The same options as <see cref="Options"/>, with indentation. Template files use these.</summary>
    public static JsonSerializerOptions IndentedOptions { get; } = Create(indented: true);

    /// <summary>Writes the document as JSON.</summary>
    /// <param name="document">The document.</param>
    /// <param name="indented"><see langword="true"/> breaks the JSON across lines.</param>
    /// <returns>The JSON.</returns>
    public static string Serialize(PageDocument document, bool indented = false) =>
        JsonSerializer.Serialize(document, indented ? IndentedOptions : Options);

    /// <summary>
    /// Reads a document. An empty or blank string gives a new document so hosts can bind a brand new field.
    /// JSON that does not parse returns invalid, with the parser message. A document that deserializes as null says "The page data is empty."
    /// A successful read is passed through <see cref="Normalize"/>.
    /// </summary>
    /// <param name="json">The stored JSON, or blank for a new document.</param>
    /// <param name="fallbackMode">The mode of a new document. The default is web.</param>
    /// <returns>The document, or invalid.</returns>
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
    /// The root type becomes <c>page</c>, a blank root id becomes <c>root</c>, and a mode other than web or email becomes web.
    /// </summary>
    /// <param name="document">The document, changed in place.</param>
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