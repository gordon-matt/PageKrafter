namespace PageKrafter.Rendering;

/// <summary>Encodes text, checks link addresses, and turns HTML into plain text for the layers panel.</summary>
public static class Html
{
    private static readonly string[] AllowedSchemes = ["http", "https", "mailto", "tel"];

    /// <summary>Encodes text for element content or a double-quoted attribute. Non-ASCII text is left readable. Empty text stays empty.</summary>
    /// <param name="value">The text. <see langword="null"/> and empty both return an empty string.</param>
    /// <returns>The encoded text.</returns>
    public static string Encode(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.AsSpan().IndexOfAny("&<>\"'") < 0)
        {
            return value;
        }

        var builder = new StringBuilder(value.Length + 16);
        foreach (char c in value)
        {
            builder.Append(c switch
            {
                '&' => "&amp;",
                '<' => "&lt;",
                '>' => "&gt;",
                '"' => "&quot;",
                '\'' => "&#39;",
                _ => c.ToString(),
            });
        }

        return builder.ToString();
    }

    /// <summary>
    /// Returns the URL when it is relative or uses http, https, mailto, or tel. Otherwise an empty string.
    /// A <c>data:image</c> URL is kept when <paramref name="allowDataImage"/> is set, except SVG.
    /// The returned text is the trimmed original. Whitespace is removed only to decide the scheme.
    /// </summary>
    /// <param name="value">The typed address.</param>
    /// <param name="allowDataImage"><see langword="true"/> also allows a non-SVG data image.</param>
    /// <returns>The trimmed URL, or an empty string.</returns>
    public static string SafeUrl(string? value, bool allowDataImage = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string url = value.Trim();
        string compact = new(url.Where(c => !char.IsWhiteSpace(c) && !char.IsControl(c)).ToArray());
        int colon = compact.IndexOf(':');
        int boundary = compact.IndexOfAny(['/', '?', '#']);
        if (colon <= 0 || (boundary >= 0 && boundary < colon))
        {
            return url;
        }

        string scheme = compact[..colon].ToLowerInvariant();
        return AllowedSchemes.Contains(scheme, StringComparer.Ordinal)
            ? url
            : allowDataImage && compact.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) && !compact.StartsWith("data:image/svg", StringComparison.OrdinalIgnoreCase)
            ? url
            : string.Empty;
    }

    /// <summary>Removes tags, replaces each tag with a space, collapses whitespace, and decodes HTML entities.</summary>
    /// <param name="html">The markup. <see langword="null"/> and empty both return an empty string.</param>
    /// <returns>The plain text.</returns>
    public static string StripTags(string? html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(html.Length);
        bool inTag = false;
        foreach (char c in html)
        {
            if (c == '<')
            {
                inTag = true;
            }
            else if (c == '>')
            {
                inTag = false;
                builder.Append(' ');
            }
            else if (!inTag)
            {
                builder.Append(c);
            }
        }

        return System.Net.WebUtility.HtmlDecode(string.Join(' ', builder.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)));
    }

    /// <summary>Leaves text that already fits. Longer text is cut, trimmed, and ends with an ellipsis.</summary>
    /// <param name="text">The text.</param>
    /// <param name="length">The longest result, including the ellipsis.</param>
    /// <returns>The text, shortened when it is longer than <paramref name="length"/>.</returns>
    public static string Truncate(string text, int length) =>
        text.Length <= length ? text : string.Concat(text.AsSpan(0, length - 1).TrimEnd(), "…");
}