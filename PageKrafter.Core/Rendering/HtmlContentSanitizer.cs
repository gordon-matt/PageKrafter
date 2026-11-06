using Ganss.Xss;

namespace PageKrafter.Rendering;

/// <summary>Cleans rich text and raw HTML from editors. Replace it in DI to apply a host's own policy.</summary>
public interface IHtmlContentSanitizer
{
    /// <summary>
    /// Sanitizes the value.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns>The string.</returns>
    string Sanitize(string html);
}

/// <summary>
/// Represents the HTML content sanitizer.
/// </summary>
public sealed class HtmlContentSanitizer : IHtmlContentSanitizer
{
    private readonly HtmlSanitizer sanitizer;

    /// <summary>
    /// Initializes a new instance of the HtmlContentSanitizer class.
    /// </summary>
    public HtmlContentSanitizer()
    {
        sanitizer = new HtmlSanitizer();
        sanitizer.AllowedAttributes.Add("class");
        sanitizer.AllowedAttributes.Add("target");
        sanitizer.AllowedAttributes.Add("rel");
        sanitizer.AllowedAttributes.Add("role");
        sanitizer.AllowedSchemes.Add("mailto");
        sanitizer.AllowedSchemes.Add("tel");
    }

    /// <summary>
    /// Sanitizes the value.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns>The string.</returns>
    public string Sanitize(string html) => string.IsNullOrWhiteSpace(html) ? string.Empty : sanitizer.Sanitize(html);
}