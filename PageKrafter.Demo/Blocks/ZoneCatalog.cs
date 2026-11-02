namespace PageKrafter.Demo.Blocks;

/// <summary>Stands in for the CMS zone service (Inferno's <c>IZoneService</c> plus its content block providers).</summary>
public interface IZoneCatalog
{
    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>The HTML the zone's content blocks produce, used for the editor preview and the demo's public page.</summary>
    Task<string> RenderAsync(string zone, CancellationToken cancellationToken = default);
}

public sealed class DemoZoneCatalog : IZoneCatalog
{
    private static readonly Dictionary<string, string> Zones = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HomePageContent"] = "<p><strong>Latest news:</strong> our new store opens on Friday.</p>",
        ["Sidebar"] = "<ul><li><a href=\"#\">Opening hours</a></li><li><a href=\"#\">Find us</a></li><li><a href=\"#\">Contact</a></li></ul>",
        ["SearchForm"] = "<form><input type=\"search\" placeholder=\"Search the site\" style=\"width:100%;padding:8px\"></form>",
        ["Footer"] = "<p>© Example Co. All rights reserved.</p>",
    };

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<string>>(Zones.Keys.Order(StringComparer.OrdinalIgnoreCase).ToArray());

    public Task<string> RenderAsync(string zone, CancellationToken cancellationToken = default) =>
        Task.FromResult(Zones.TryGetValue(zone, out string? html) ? html : string.Empty);
}
