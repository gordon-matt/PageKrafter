using Ganss.Xss;

namespace PageKrafter.Rendering;

/// <summary>Cleans rich text and raw HTML from editors. Replace it in DI to apply a host's own policy.</summary>
public interface IHtmlContentSanitizer
{
    /// <summary>Returns the HTML the host's policy allows.</summary>
    /// <param name="html">The markup from an editor.</param>
    /// <returns>The cleaned markup.</returns>
    string Sanitize(string html);
}

/// <summary>
/// The default sanitizer. It uses <see cref="Ganss.Xss.HtmlSanitizer"/> and also allows the class, target, rel, and role attributes and the mailto and tel schemes.
/// </summary>
public sealed class HtmlContentSanitizer : IHtmlContentSanitizer
{
    private readonly HtmlSanitizer sanitizer;

    /// <summary>Creates the sanitizer with those extra attributes and schemes.</summary>
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

    /// <summary>Cleans the markup. Blank or white space returns an empty string.</summary>
    /// <param name="html">The markup from an editor.</param>
    /// <returns>The cleaned markup, or an empty string.</returns>
    public string Sanitize(string html) => string.IsNullOrWhiteSpace(html) ? string.Empty : sanitizer.Sanitize(html);
}