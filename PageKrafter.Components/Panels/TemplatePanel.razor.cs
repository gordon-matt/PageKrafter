using System.Text.Json;
using Microsoft.AspNetCore.Components.Web;

namespace PageKrafter.Components.Panels;

/// <summary>Section templates that can be dragged onto the page, and page templates that replace the document. Saved templates can be deleted. Built-in ones cannot.</summary>
public partial class TemplatePanel
{
    /// <summary>The editor that inserts or applies the template.</summary>
    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;

    private IEnumerable<IGrouping<string, PageTemplate>> Sections => Editor.Templates
        .Where(template => template.Kind == TemplateKind.Section)
        .GroupBy(template => template.Category);

    private IEnumerable<PageTemplate> Pages => Editor.Templates.Where(template => template.Kind == TemplateKind.Page);

    private static string Payload(PageTemplate template) => JsonSerializer.Serialize(new { kind = "template", templateId = template.Id });

    private static string Tooltip(PageTemplate template) => template.Description.Length > 0 ? template.Description : template.Name;

    private static async Task OnKeyDownAsync(KeyboardEventArgs args, Func<Task> action)
    {
        if (args.Key is "Enter" or " ")
        {
            await action();
        }
    }
}