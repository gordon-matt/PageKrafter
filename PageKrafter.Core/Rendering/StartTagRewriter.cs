using System.Text.RegularExpressions;

namespace PageKrafter.Rendering;

/// <summary>
/// Adds attributes to the first element of a block's HTML. This lets the renderer stamp selection
/// ids and common styles on any block, including host blocks and Razor components, without a
/// wrapper element that would break flex layouts and email tables.
/// </summary>
internal static partial class StartTagRewriter
{
    /// <summary>Returns null when the HTML does not start with an element (ignoring whitespace and comments).</summary>
    public static string? Apply(string html, string cssClass, string style, IReadOnlyList<(string Name, string Value)> attributes)
    {
        int start = FindStartTag(html);
        if (start < 0)
        {
            return null;
        }

        int nameEnd = start + 1;
        while (nameEnd < html.Length && (char.IsAsciiLetterOrDigit(html[nameEnd]) || html[nameEnd] is '-' or ':' or '_'))
        {
            nameEnd++;
        }

        int end = FindTagEnd(html, nameEnd);
        if (end < 0)
        {
            return null;
        }

        string tagName = html[(start + 1)..nameEnd];
        string attributeText = html[nameEnd..end].TrimEnd();
        bool selfClosing = attributeText.EndsWith('/');
        if (selfClosing)
        {
            attributeText = attributeText[..^1];
        }

        var existing = ParseAttributes(attributeText);
        if (cssClass.Length > 0)
        {
            Merge(existing, "class", Html.Encode(cssClass), (current, added) => $"{current} {added}".Trim());
        }

        if (style.Length > 0)
        {
            Merge(existing, "style", Html.Encode(style), (current, added) => $"{current.Trim().TrimEnd(';')};{added}".TrimStart(';'));
        }

        foreach (var (name, value) in attributes)
        {
            Merge(existing, name, Html.Encode(value), (_, added) => added);
        }

        var builder = new StringBuilder(html.Length + 96);
        builder.Append(html, 0, start).Append('<').Append(tagName);
        foreach (var (name, value) in existing)
        {
            builder.Append(' ').Append(name);
            if (value is not null)
            {
                builder.Append("=\"").Append(value).Append('"');
            }
        }

        if (selfClosing)
        {
            builder.Append(" /");
        }

        builder.Append('>').Append(html, end + 1, html.Length - end - 1);
        return builder.ToString();
    }

    private static int FindStartTag(string html)
    {
        int i = 0;
        while (i < html.Length)
        {
            if (char.IsWhiteSpace(html[i]))
            {
                i++;
                continue;
            }

            if (string.CompareOrdinal(html, i, "<!--", 0, 4) == 0)
            {
                int close = html.IndexOf("-->", i + 4, StringComparison.Ordinal);
                if (close < 0)
                {
                    return -1;
                }

                i = close + 3;
                continue;
            }

            return html[i] == '<' && i + 1 < html.Length && char.IsAsciiLetter(html[i + 1]) ? i : -1;
        }

        return -1;
    }

    private static int FindTagEnd(string html, int from)
    {
        char quote = '\0';
        for (int i = from; i < html.Length; i++)
        {
            char c = html[i];
            if (quote != '\0')
            {
                if (c == quote)
                {
                    quote = '\0';
                }
            }
            else if (c is '"' or '\'')
            {
                quote = c;
            }
            else if (c == '>')
            {
                return i;
            }
        }

        return -1;
    }

    private static List<(string Name, string? Value)> ParseAttributes(string text)
    {
        var attributes = new List<(string Name, string? Value)>();
        foreach (Match match in AttributePattern().Matches(text))
        {
            var value = match.Groups["value"];
            attributes.Add((match.Groups["name"].Value, value.Success ? value.Value.Replace("\"", "&quot;") : null));
        }

        return attributes;
    }

    private static void Merge(List<(string Name, string? Value)> attributes, string name, string value, Func<string, string, string> combine)
    {
        int index = attributes.FindIndex(attribute => string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            attributes.Add((name, value));
        }
        else
        {
            attributes[index] = (attributes[index].Name, combine(attributes[index].Value ?? string.Empty, value));
        }
    }

    [GeneratedRegex("""(?<name>[^\s"'>/=]+)(?:\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)'|(?<value>[^\s"'=<>`]+)))?""")]
    private static partial Regex AttributePattern();
}