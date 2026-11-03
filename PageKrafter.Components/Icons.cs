namespace PageKrafter.Components;

internal static class Icons
{
    private const string Open = """<svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">""";
    private const string Close = "</svg>";

    public static readonly MarkupString Undo = new(Open + """<path d="M9 14 4 9l5-5"/><path d="M4 9h11a5 5 0 0 1 0 10h-3"/>""" + Close);
    public static readonly MarkupString Redo = new(Open + """<path d="m15 14 5-5-5-5"/><path d="M20 9H9a5 5 0 0 0 0 10h3"/>""" + Close);
    public static readonly MarkupString Desktop = new(Open + """<rect x="3" y="4" width="18" height="12" rx="2"/><path d="M8 20h8M12 16v4"/>""" + Close);
    public static readonly MarkupString Tablet = new(Open + """<rect x="5" y="3" width="14" height="18" rx="2"/><path d="M11 18h2"/>""" + Close);
    public static readonly MarkupString Mobile = new(Open + """<rect x="7" y="3" width="10" height="18" rx="2"/><path d="M11 18h2"/>""" + Close);
    public static readonly MarkupString Eye = new(Open + """<path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z"/><circle cx="12" cy="12" r="3"/>""" + Close);
    public static readonly MarkupString Pencil = new(Open + """<path d="M4 20h4L19 9l-4-4L4 16z"/><path d="m13.5 6.5 4 4"/>""" + Close);
    public static readonly MarkupString Code = new(Open + """<path d="m8 8-4 4 4 4M16 8l4 4-4 4"/>""" + Close);
    public static readonly MarkupString Trash = new(Open + """<path d="M4 7h16M10 11v6M14 11v6M6 7l1 13h10l1-13M9 7V4h6v3"/>""" + Close);
    public static readonly MarkupString Copy = new(Open + """<rect x="8" y="8" width="12" height="12" rx="2"/><path d="M16 8V5a1 1 0 0 0-1-1H5a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h3"/>""" + Close);
    public static readonly MarkupString Parent = new(Open + """<path d="M12 19V5M6 11l6-6 6 6"/>""" + Close);
    public static readonly MarkupString Star = new(Open + """<path d="m12 3 2.8 5.7 6.2.9-4.5 4.4 1 6.2L12 17.3 6.5 20.2l1-6.2L3 9.6l6.2-.9z"/>""" + Close);
    public static readonly MarkupString Folder = new(Open + """<path d="M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>""" + Close);
    public static readonly MarkupString X = new(Open + """<path d="M6 6l12 12M18 6 6 18"/>""" + Close);
    public static readonly MarkupString Chevron = new(Open + """<path d="m9 6 6 6-6 6"/>""" + Close);
    public static readonly MarkupString Up = new(Open + """<path d="m6 15 6-6 6 6"/>""" + Close);
    public static readonly MarkupString Down = new(Open + """<path d="m6 9 6 6 6-6"/>""" + Close);
    public static readonly MarkupString Settings = new(Open + """<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.8l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-1.8-.3 1.7 1.7 0 0 0-1 1.5V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-1.1-1.5 1.7 1.7 0 0 0-1.8.3l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0 .3-1.8 1.7 1.7 0 0 0-1.5-1H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.5-1.1 1.7 1.7 0 0 0-.3-1.8l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.8.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.8-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.8V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1z"/>""" + Close);
}
