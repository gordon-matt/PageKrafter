using System.Text.Json;
using Microsoft.AspNetCore.Components.Web;

namespace PageKrafter.Components.Panels;

public partial class BlockPalette
{
    private string search = string.Empty;

    [Parameter, EditorRequired]
    public PageKrafterEditor Editor { get; set; } = default!;

    private IReadOnlyList<IGrouping<string, BlockDefinition>> Groups => Editor.Blocks
        .PaletteFor(Editor.Session.Document.Mode)
        .Where(Editor.Session.IsAllowed)
        .Where(Matches)
        .GroupBy(block => block.Category)
        .ToArray();

    private bool Matches(BlockDefinition block)
    {
        string term = search.Trim();
        return term.Length == 0
            || block.Label.Contains(term, StringComparison.OrdinalIgnoreCase)
            || block.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
            || block.Description.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private static string Payload(BlockDefinition block) => JsonSerializer.Serialize(new { kind = "block", blockType = block.Type });

    private static string Tooltip(BlockDefinition block) => block.Description.Length > 0 ? block.Description : block.Label;

    private async Task OnKeyDownAsync(KeyboardEventArgs args, BlockDefinition block)
    {
        if (args.Key is "Enter" or " ")
        {
            await Editor.InsertBlockAsync(block.Type);
        }
    }
}