namespace PageKrafter.Components;

/// <summary>A message from the canvas iframe, relayed by pagekrafter.js. One flat shape keeps the JS side simple.</summary>
public sealed record CanvasMessage
{
    public string Type { get; init; } = string.Empty;

    public string? Id { get; init; }

    public string? ParentId { get; init; }

    public string? Slot { get; init; }

    public int Index { get; init; }

    public string? Kind { get; init; }

    public string? BlockType { get; init; }

    public string? TemplateId { get; init; }

    public string? Prop { get; init; }

    public string? Value { get; init; }

    public string? Command { get; init; }
}

internal static class CanvasDocument
{
    private const string AssetRoot = "_content/PageKrafter.Components/";

    // Bumped when the editor assets change, so a browser does not keep an older stylesheet at the same address.
    private static readonly string AssetVersion = typeof(CanvasDocument).Assembly.GetName().Version?.ToString() ?? "1";

    public static string Build(string bodyHtml, DocumentMode mode, string baseUri, IEnumerable<string> styles, string? headHtml)
    {
        var links = new System.Text.StringBuilder();
        if (mode == DocumentMode.Web)
        {
            foreach (string style in styles.Where(style => !string.IsNullOrWhiteSpace(style)).Distinct(StringComparer.Ordinal))
            {
                links.Append($"""<link rel="stylesheet" href="{Html.Encode(style)}">""");
            }

            links.Append(headHtml);
        }

        string modeClass = mode == DocumentMode.Email ? "pk-mode-email" : "pk-mode-web";
        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <base href="{Html.Encode(baseUri)}">
            {links}
            <style>{BaseStyles.For(mode)}</style>
            <link rel="stylesheet" href="{AssetRoot}pagekrafter-canvas.css?v={AssetVersion}">
            </head>
            <body class="pk-canvas {modeClass}">
            <div id="pk-canvas-root">{bodyHtml}</div>
            <script src="{AssetRoot}pagekrafter-canvas.js?v={AssetVersion}"></script>
            </body>
            </html>
            """;
    }

    public static string ModuleUrl => $"./{AssetRoot}pagekrafter.js?v={AssetVersion}";

    public static string StylesheetUrl => $"{AssetRoot}pagekrafter.css?v={AssetVersion}";
}