using System.Text.RegularExpressions;

namespace PageKrafter.Rendering;

/// <summary>
/// Cleans values typed into style fields before they go into a style attribute. Editors are trusted
/// users, so this only stops a value from escaping its declaration, not every odd CSS value.
/// </summary>
public static partial class Css
{
    /// <summary>
    /// Gets the clean.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The clean.</returns>
    public static string Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string trimmed = value.Trim();
        return trimmed.Contains("expression(", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("@import", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : new string(trimmed.Where(c => c is not (';' or '{' or '}' or '<' or '>' or '"' or '\\') && !char.IsControl(c)).ToArray()).Trim();
    }

    /// <summary>
    /// Gets the URL value.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The URL value.</returns>
    public static string UrlValue(string? url)
    {
        string safe = Html.SafeUrl(url, allowDataImage: true);
        return safe.Length == 0
            ? string.Empty
            : $"url('{safe.Replace("'", "%27").Replace("(", "%28").Replace(")", "%29").Replace("\"", "%22").Replace("\\", "%5C")}')";
    }

    /// <summary>
    /// Gets the class names.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The class names.</returns>
    public static string ClassNames(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(name => ClassNamePattern().IsMatch(name)));

    /// <summary>
    /// Gets the identifier.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The identifier.</returns>
    public static string Identifier(string? value) =>
        !string.IsNullOrWhiteSpace(value) && IdentifierPattern().IsMatch(value.Trim()) ? value.Trim() : string.Empty;

    /// <summary>Joins declarations, skipping any whose value is empty after cleaning.</summary>
    public static string Declarations(params (string Property, string? Value)[] declarations)
    {
        var builder = new StringBuilder();
        foreach (var (property, value) in declarations)
        {
            string clean = property == "background-image" ? value ?? string.Empty : Clean(value);
            if (clean.Length > 0)
            {
                builder.Append(property).Append(':').Append(clean).Append(';');
            }
        }

        return builder.ToString();
    }

    /// <summary>Reads a pixel count from values like <c>24px</c> or <c>24</c>. Email tables need plain numbers.</summary>
    public static int? Pixels(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        if (trimmed.EndsWith("px", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[..^2];
        }

        return decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal pixels)
            ? (int)Math.Round(pixels)
            : null;
    }

    [GeneratedRegex("^-?[A-Za-z_][A-Za-z0-9_-]*$")]
    private static partial Regex ClassNamePattern();

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_:.-]*$")]
    private static partial Regex IdentifierPattern();
}