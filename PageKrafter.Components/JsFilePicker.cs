namespace PageKrafter.Components;

/// <summary>
/// An <see cref="IFilePicker"/> for file managers that run in the browser. It calls a global JavaScript function,
/// passes it <c>{ kind, title, current }</c> (kind is <c>"any"</c>, <c>"image"</c> or <c>"video"</c>) and waits for
/// the promise it returns. The promise resolves to <c>{ url, name }</c> for the chosen file, or <c>null</c> if the
/// editor closed the file manager. Register it with <c>AddPageKrafterJsFilePicker("MyApp.pickFile")</c>.
/// </summary>
/// <remarks>
/// Derive from it to send the function more than the request, such as a folder that depends on who is signed in.
/// </remarks>
public class JsFilePicker(IJSRuntime js, string function) : IFilePicker
{
    /// <summary>
    /// Gets the pick.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the picked file.</returns>
    public virtual async ValueTask<PickedFile?> PickAsync(FilePickRequest request, CancellationToken cancellationToken = default)
    {
        var picked = await js.InvokeAsync<PickedFile?>(function, cancellationToken, CreateArgument(request));
        return string.IsNullOrWhiteSpace(picked?.Url) ? null : picked;
    }

    /// <summary>The single argument passed to the JavaScript function.</summary>
    protected virtual object CreateArgument(FilePickRequest request) => new Dictionary<string, object?>
    {
        ["kind"] = request.Kind.ToString().ToLowerInvariant(),
        ["title"] = request.Title,
        ["current"] = request.Current,
    };
}