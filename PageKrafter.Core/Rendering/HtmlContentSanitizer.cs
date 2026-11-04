using Ganss.Xss;

namespace PageKrafter.Rendering;

/// <summary>Cleans rich text and raw HTML from editors. Replace it in DI to apply a host's own policy.</summary>
public interface IHtmlContentSanitizer
{
    string Sanitize(string html);
}

public sealed class HtmlContentSanitizer : IHtmlContentSanitizer
{
    private readonly HtmlSanitizer sanitizer;

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

    public string Sanitize(string html) => string.IsNullOrWhiteSpace(html) ? string.Empty : sanitizer.Sanitize(html);
}