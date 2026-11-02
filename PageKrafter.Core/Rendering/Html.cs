namespace PageKrafter.Rendering;

public static class Html
{
    private static readonly string[] AllowedSchemes = ["http", "https", "mailto", "tel"];

    /// <summary>Encodes text for element content or a double-quoted attribute. Non-ASCII text is left readable.</summary>
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

    /// <summary>Returns the URL when it is relative or uses a safe scheme, otherwise an empty string.</summary>
    public static string SafeUrl(string? value, bool allowDataImage = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string url = value.Trim();
        string compact = new string(url.Where(c => !char.IsWhiteSpace(c) && !char.IsControl(c)).ToArray());
        int colon = compact.IndexOf(':');
        int boundary = compact.IndexOfAny(['/', '?', '#']);
        if (colon <= 0 || (boundary >= 0 && boundary < colon))
        {
            return url;
        }

        string scheme = compact[..colon].ToLowerInvariant();
        if (AllowedSchemes.Contains(scheme, StringComparer.Ordinal))
        {
            return url;
        }

        return allowDataImage && compact.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) && !compact.StartsWith("data:image/svg", StringComparison.OrdinalIgnoreCase)
            ? url
            : string.Empty;
    }

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

    public static string Truncate(string text, int length) =>
        text.Length <= length ? text : string.Concat(text.AsSpan(0, length - 1).TrimEnd(), "…");
}
