namespace PageKrafter.Blocks;

public static class BlockIcons
{
    private const string Open = """<svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">""";
    private const string Close = "</svg>";

    public const string Square = Open + """<rect x="4" y="4" width="16" height="16" rx="2"/>""" + Close;
    public const string Page = Open + """<path d="M6 3h8l4 4v14H6z"/><path d="M14 3v4h4"/>""" + Close;
    public const string Section = Open + """<rect x="3" y="5" width="18" height="14" rx="2"/><path d="M7 10h10M7 14h6"/>""" + Close;
    public const string Columns = Open + """<rect x="3" y="5" width="7.5" height="14" rx="1.5"/><rect x="13.5" y="5" width="7.5" height="14" rx="1.5"/>""" + Close;
    public const string Container = Open + """<rect x="4" y="4" width="16" height="16" rx="2" stroke-dasharray="3 2.5"/>""" + Close;
    public const string Heading = Open + """<path d="M6 4v16M18 4v16M6 12h12"/>""" + Close;
    public const string Text = Open + """<path d="M4 6h16M4 10h16M4 14h16M4 18h10"/>""" + Close;
    public const string Image = Open + """<rect x="3" y="4" width="18" height="16" rx="2"/><circle cx="9" cy="10" r="2"/><path d="m21 16-5-5-9 9"/>""" + Close;
    public const string Button = Open + """<rect x="3" y="7" width="18" height="10" rx="5"/><path d="M9 12h6"/>""" + Close;
    public const string Divider = Open + """<path d="M3 12h18"/><path d="M8 7h8M8 17h8" opacity=".4"/>""" + Close;
    public const string Spacer = Open + """<path d="M12 4v16M8 8l4-4 4 4M8 16l4 4 4-4"/>""" + Close;
    public const string Video = Open + """<rect x="3" y="5" width="18" height="14" rx="2"/><path d="m10 9 5 3-5 3z"/>""" + Close;
    public const string Html = Open + """<path d="m8 8-4 4 4 4M16 8l4 4-4 4M14 5l-4 14"/>""" + Close;
    public const string Zone = Open + """<rect x="3" y="3" width="18" height="18" rx="2"/><path d="M3 9h18M9 21V9"/>""" + Close;
    public const string Callout = Open + """<path d="M4 5h16v11H9l-5 4z"/><path d="M12 8v3M12 13.5v.5"/>""" + Close;
}
