using System.Text;
using System.Text.RegularExpressions;
using PageKrafter.Demo.Storage;

namespace PageKrafter.Demo.Components.Pages;

public partial class Preview
{
    private string? html;
    private bool email;

    [Inject]
    private DemoPageStore Pages { get; set; } = default!;

    [Inject]
    private PageRenderer Renderer { get; set; } = default!;

    [Inject]
    private IZoneCatalog Zones { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Parameter]
    public string Name { get; set; } = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        string? json = await Pages.LoadAsync(Name);
        var result = PageKrafterJson.Deserialize(json ?? string.Empty);
        if (json is null || !result.IsSuccess)
        {
            html = null;
            return;
        }

        var document = result.Value;
        email = document.Mode == DocumentMode.Email;
        string page = await Renderer.RenderDocumentAsync(document, Name);
        html = email ? page : await AddSiteChromeAsync(page);
    }

    // What the CMS page view does with the saved HTML: link the site's stylesheet and fill each zone.
    private async Task<string> AddSiteChromeAsync(string page)
    {
        page = page.Replace("</head>", $"<base href=\"{Navigation.BaseUri}\"><link rel=\"stylesheet\" href=\"css/site-content.css\"></head>", StringComparison.OrdinalIgnoreCase);

        var output = new StringBuilder();
        int last = 0;
        foreach (Match match in ZoneToken().Matches(page))
        {
            output.Append(page, last, match.Index - last);
            output.Append(await Zones.RenderAsync(match.Groups["Zone"].Value));
            last = match.Index + match.Length;
        }

        return output.Append(page, last, page.Length - last).ToString();
    }

    [GeneratedRegex(@"\[\[ContentZone:(?<Zone>[^\]]+)\]\]")]
    private static partial Regex ZoneToken();
}
