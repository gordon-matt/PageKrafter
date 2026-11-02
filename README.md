# PageKrafter

A drag-and-drop page and email builder for Blazor Server.

- **The page is a JSON tree.** That is all you store. HTML is generated from it, so pages stay editable and can be re-rendered when blocks improve.
- **The canvas is an isolated iframe** loaded with `srcdoc`, so your site's CSS applies to the page being edited and the editor's CSS never leaks into it. The editor and canvas talk over `postMessage`.
- **Web and email from the same blocks.** In email mode, blocks render as tables with inline styles.
- **Templates.** Built-in section and page templates (two, three and four columns, sidebars, hero, newsletter and more), plus templates your users save themselves.
- **Your own blocks.** Write a block in C# or as a Razor component, give it fields and child slots, and register it with one line.

## Projects

| Project | Purpose |
| --- | --- |
| `PageKrafter.Core` | Document model, block registry, built-in blocks, templates, HTML rendering and the editing session. No Blazor dependency, so a public site can render pages with only this. |
| `PageKrafter.Components` | The `PageKrafterEditor` Blazor component, the canvas scripts and the extension base classes for Razor blocks and inspector fields. |
| `PageKrafter.Demo` | A Blazor Server app with a page builder, an email builder and two example host blocks (a CMS content zone and a callout). |
| `PageKrafter.Tests` | xUnit tests. |

Run the demo with `dotnet run --project PageKrafter.Demo` and open http://localhost:5180.

## Getting started

Register the services:

```csharp
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddPageKrafter(options =>
{
    // Your site's stylesheet, loaded into the canvas so pages look the same while editing.
    options.CanvasStyles.Add("css/site.css");
});
```

Add the editor stylesheet to `App.razor`:

```html
<link rel="stylesheet" href="@Assets["_content/PageKrafter.Components/pagekrafter.css"]" />
```

Use the editor in an interactive page:

```razor
<PageKrafterEditor @bind-Value="page.Json" OnSave="SaveAsync" />

@code {
    private async Task SaveAsync(PageDocument document)
    {
        page.Json = PageKrafterJson.Serialize(document);
        page.Html = await Renderer.RenderAsync(document);   // optional: store ready-made HTML too
        await db.SaveChangesAsync();
    }
}
```

`Value` is the JSON string. An empty value starts a new page. Set `Mode="DocumentMode.Email"` for an email. Other parameters:

- `Height` sets the editor height.
- `AllowedBlocks` restricts the palette.
- `ShowTemplates` hides or shows the templates tab.
- `ToolbarContent` adds your own toolbar buttons.

To show a page, either store the HTML from `PageRenderer.RenderAsync` when saving, or keep only the JSON and render it at request time. Use `PageRenderer.RenderDocumentAsync` for a complete HTML document, which is what an email send needs.

### Editor features

- Drag blocks, section templates and layers. Drop near a container's edge to place a block beside it rather than inside.
- Click a palette item to add it next to the selected block.
- Double-click headings, text and buttons to edit them on the page. Text blocks get a small formatting toolbar.
- Inspector with grouped content, style and advanced fields. Every block gets padding, margin, colours, alignment, radius, CSS classes, an anchor id and responsive visibility.
- Layers tree, undo and redo (edits to the same field are merged), desktop, tablet and mobile widths, preview mode and an HTML export dialog.
- Keyboard shortcuts:
  - Ctrl+Z to undo.
  - Ctrl+Y or Ctrl+Shift+Z to redo.
  - Ctrl+D to duplicate.
  - Delete to remove the selected block.
  - Ctrl+Up and Ctrl+Down to move the selected block.
  - Escape to select the parent block.

## Templates

There are two kinds of template:

- **Section** templates are a block and its children. They are dragged into the page like blocks.
- **Page** templates replace the whole document. Applying one can be undone.

Users save the selected block or the whole page with the star button. Saved templates go to an `ITemplateStore`:

```csharp
services.AddPageKrafterJsonFileTemplates(Path.Combine(env.ContentRootPath, "App_Data", "templates"));
// or your own, for example backed by EF Core (registered as scoped):
services.AddPageKrafterTemplateStore<DbTemplateStore>();
```

The default store is in memory. To ship read-only templates, implement `ITemplateProvider` and register it with `AddPageKrafterTemplates<T>()`. The `Nodes` helpers make the trees easy to build:

```csharp
public sealed class CompanyTemplates : ITemplateProvider
{
    public Task<IReadOnlyList<PageTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PageTemplate>>(
        [
            new PageTemplate
            {
                Id = "company-intro",
                Name = "Intro row",
                Category = "Company",
                Kind = TemplateKind.Section,
                Content = Nodes.Section(Nodes.Columns("1:2",
                    [Nodes.Image()],
                    [Nodes.Heading("About us"), Nodes.Text("<p>…</p>")])),
            },
        ]);
}
```

## Custom blocks

A block is a `BlockDefinition`. It is resolved from DI per scope, so it can inject your services.

```csharp
public sealed class QuoteBlock : BlockDefinition
{
    public override string Type => "acme.quote";       // prefix your types so they never clash with built-ins
    public override string Label => "Quote";
    public override string Category => "Acme";
    public override DocumentMode Modes => DocumentMode.All;   // web only by default

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        BlockField.TextArea("quote", "Quote", "Simplicity is the soul of efficiency."),
        BlockField.Text("author", "Author"),
    ];

    public override ValueTask<string> RenderAsync(BlockRenderContext context) => ValueTask.FromResult($"""
        <blockquote>
          <p{context.EditableAttributes("quote")}>{context.Text("quote")}</p>
          <cite>{context.Text("author")}</cite>
        </blockquote>
        """);
}

services.AddPageKrafterBlock<QuoteBlock>();
```

Things to know:

- **Render one root element.** The renderer adds the common styles to it. In the editor it also adds the selection attributes, so blocks need no wrapper element (which would break flex rows and email tables).
- **Encode what you output.** `context.Text`, `context.Url` and `context.RichText` encode, check and sanitize values for you. Use `context.IsEmail` and `context.IsEditor` to vary the output.
- **Child blocks.** Override `GetSlots`, put `context.SlotAttributes(name)` on the containing element, and output `await context.RenderSlotAsync(name)` inside it. `CanContain` and `CanBePlacedIn` set nesting rules.
- **Dynamic options.** Set `HasDynamicOptions = true` on a select field and override `GetOptionsAsync`. `OptionsDependOn` lists the props that should reload the options.
- **Keeping props consistent.** `OnPropertyChanged` lets a block keep related props in step. For example, Columns moves blocks out of removed columns.
- **Replacing or removing built-ins.** Registering a block with a built-in's type name replaces the built-in. `options.DisabledBlocks.Add(BlockTypes.Video)` removes one.

### Razor component blocks

Derive from `ComponentBlockDefinition<TComponent>` and write the markup in a component that inherits `BlockComponentBase`:

```razor
@inherits BlockComponentBase
<aside class="callout">
    <strong @attributes="@(Editable("title"))">@Value("title")</strong>
    <div @attributes="@(SlotAttributes())">@Slot()</div>
</aside>
```

### Custom inspector fields

Make a component that inherits `FieldEditorBase` and calls `SetValueAsync`. Register it with `services.AddPageKrafterFieldEditor<TonePicker>("acme.tone")`, then use `Editor = "acme.tone"` on a field. Registering a built-in name such as `"image"` replaces that editor everywhere. For example, you could use it to plug in your media library.

To replace a block's whole content section in the inspector, set `EditorComponent` to a component that inherits `BlockEditorBase`.

## Using it in Inferno CMS or MantleCMS

The demo's `ContentZoneBlock` shows the pattern. Editors place a **Content zone** block anywhere in the page and pick the zone from a list that comes from the CMS. In the editor, the block previews the zone's current content. In exported HTML, it writes the token both CMSs already understand:

```
[[ContentZone:Sidebar]]
```

Inferno's `StandardPageView` (and Mantle's equivalent) replace that token with the zone's content blocks. So the simplest integration is:

1. Keep the PageKrafter JSON in a new column on the page version.
2. When saving, store `PageRenderer.RenderAsync(document)` in the existing HTML field.

Nothing else in the CMS has to change.

One caveat: Inferno splits the page HTML at each token and renders the pieces as separate `MarkupString`s. That is fine for static server rendering. In an interactive render mode, though, a token inside a column leaves each piece with unclosed tags, and the browser auto-closes them, which breaks the layout. If pages render interactively, use one of these approaches:

- Replace the tokens with the zones' HTML in one pass, then output a single `MarkupString`. The demo's preview page does this.
- Better, render the JSON at request time with a content zone block whose `RenderAsync` asks the CMS zone service for the zone's HTML directly. The token is then not needed at all.

## Security

- Rich text and HTML blocks go through HtmlSanitizer.
- URLs are limited to http, https, mailto, tel and relative paths.
- Style values are cleaned so they can't break out of a declaration.
- Template files can only be read and written inside their folder.

The canvas iframe is sandboxed with `allow-scripts allow-same-origin allow-modals`. The editor needs same-origin access to the iframe for drag and drop and inline editing. That is safe because everything rendered in the canvas has already been sanitized.

## Design notes

PageKrafter borrows the best ideas from other builders:

- From **Puck**: the JSON tree as the source of truth, typed fields per block, and slots for nested content.
- From **GrapesJS**: the iframe canvas and the in-canvas selection toolbar.
- From **VvvebJs**: `srcdoc` loading and editing in place.
- From **Silex**: the template workflow and the thumbnails drawn from the block tree.

Undo works on whole-document snapshots, because pages are small and snapshots are far more robust than replaying operations.

Not built yet:

- Global or shared blocks (symbols).
- Typed (non-string) props.
- Multi-select.
- Collaborative editing.
