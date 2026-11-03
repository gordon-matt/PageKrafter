namespace PageKrafter.Files;

/// <summary>
/// Lets editors choose a file from whatever file manager the host uses, such as a media library, a cloud
/// drive or a folder of uploads. PageKrafter has no file manager of its own. Register an implementation with
/// <c>AddPageKrafterFilePicker&lt;T&gt;()</c> and image fields (and any field marked with
/// <see cref="BlockField.FilePicker"/>) show a Browse button. Without one the fields stay plain text boxes.
/// </summary>
public interface IFilePicker
{
    /// <summary>
    /// Opens the file manager and waits for the editor to choose a file.
    /// Returns <see langword="null"/> when the editor closes it without choosing.
    /// </summary>
    ValueTask<PickedFile?> PickAsync(FilePickRequest request, CancellationToken cancellationToken = default);
}

/// <summary>What the editor wants to choose.</summary>
/// <param name="Kind">The kind of file, so the file manager can filter what it shows.</param>
/// <param name="Title">A heading for the file manager, such as the field's label.</param>
/// <param name="Current">The field's current value, so a file manager can open at, or highlight, the file already chosen.</param>
public sealed record FilePickRequest(FilePickerKind Kind, string? Title = null, string? Current = null);

/// <summary>The file the editor chose.</summary>
/// <param name="Url">What is stored in the field. Root-relative paths such as <c>/media/photo.jpg</c> are fine for pages. Use absolute URLs for emails, because mail clients can't resolve relative ones.</param>
/// <param name="Name">The file's name, when the file manager knows it.</param>
public sealed record PickedFile(string Url, string? Name = null);

public enum FilePickerKind
{
    Any,
    Image,
    Video,
}
