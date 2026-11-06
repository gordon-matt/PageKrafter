namespace PageKrafter.Rendering;

/// <summary>
/// Represents the page renderer.
/// </summary>
public sealed class PageRenderer(BlockRegistry registry, IHtmlContentSanitizer sanitizer, IServiceProvider services)
{
    private const int MaxDepth = 48;

    /// <summary>Renders the page body. Store this in a CMS field, or render it in a host page.</summary>
    public async Task<string> RenderAsync(PageDocument document, RenderOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= RenderOptions.Export;
        string body = await RenderNodeAsync(document.Root, document, options, 0, cancellationToken);
        return options.IncludeBaseStyles ? $"<style>{BaseStyles.For(document.Mode)}</style>{body}" : body;
    }

    /// <summary>Renders a complete HTML document, which is what an email send or a standalone page needs.</summary>
    public async Task<string> RenderDocumentAsync(PageDocument document, string title = "", CancellationToken cancellationToken = default)
    {
        string body = await RenderAsync(document, RenderOptions.Export with { IncludeBaseStyles = false }, cancellationToken);
        string head = document.Mode == DocumentMode.Email
            ? """<meta http-equiv="X-UA-Compatible" content="IE=edge"><meta name="x-apple-disable-message-reformatting"><meta name="format-detection" content="telephone=no,address=no,email=no,date=no">"""
            : string.Empty;

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            {head}<title>{Html.Encode(title)}</title>
            <style>{BaseStyles.For(document.Mode)}</style>
            </head>
            <body style="margin:0;padding:0;">
            {body}
            </body>
            </html>
            """;
    }

    internal async Task<string> RenderNodeAsync(BlockNode node, PageDocument document, RenderOptions options, int depth, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool editor = options.Purpose == RenderPurpose.Editor;
        if (depth > MaxDepth)
        {
            return string.Empty;
        }

        var definition = registry.Find(node.Type);
        if (definition is null)
        {
            return editor ? Placeholder(node, node.Type, $"Unknown block \"{node.Type}\". It is kept in the page but not shown.") : string.Empty;
        }

        if (!definition.Modes.HasFlag(document.Mode))
        {
            return editor ? Placeholder(node, definition.Label, $"{definition.Label} isn't available in {document.Mode.ToString().ToLowerInvariant()} documents.") : string.Empty;
        }

        var context = new BlockRenderContext(this, node, definition, document, options, depth, services, sanitizer, cancellationToken);
        string html;
        try
        {
            html = await definition.RenderAsync(context);
        }
        catch (Exception ex) when (editor && ex is not OperationCanceledException)
        {
            return Placeholder(node, definition.Label, $"{definition.Label} couldn't be shown: {ex.Message}");
        }

        var styles = definition.UsesCommonStyles
            ? CommonStyles.Resolve(node, document.Mode, definition.SelfAppliedStyles(document.Mode))
            : default;

        var attributes = new List<(string Name, string Value)>();
        if (!string.IsNullOrEmpty(styles.Id))
        {
            attributes.Add(("id", styles.Id));
        }

        if (editor)
        {
            attributes.AddRange(EditorAttributes(node, definition.Label));
            if (ReferenceEquals(node, document.Root))
            {
                attributes.Add(("data-pk-root", string.Empty));
            }

            if (string.IsNullOrWhiteSpace(html))
            {
                return Placeholder(node, definition.Label, definition.Label, "pk-empty-block");
            }
        }

        string cssClass = styles.CssClass ?? string.Empty;
        string style = styles.Style ?? string.Empty;
        if (cssClass.Length == 0 && style.Length == 0 && attributes.Count == 0)
        {
            return html;
        }

        string? rewritten = StartTagRewriter.Apply(html, cssClass, style, attributes);
        return rewritten is not null
            ? rewritten
            : editor ? StartTagRewriter.Apply($"<div class=\"pk-wrap\">{html}</div>", cssClass, style, attributes)! : html;
    }

    private static IEnumerable<(string Name, string Value)> EditorAttributes(BlockNode node, string label) =>
    [
        ("data-pk-id", node.Id),
        ("data-pk-type", node.Type),
        ("data-pk-label", label),
    ];

    private static string Placeholder(BlockNode node, string label, string message, string cssClass = "pk-missing") =>
        $"<div class=\"{cssClass}\" data-pk-id=\"{Html.Encode(node.Id)}\" data-pk-type=\"{Html.Encode(node.Type)}\" data-pk-label=\"{Html.Encode(label)}\">{Html.Encode(message)}</div>";
}