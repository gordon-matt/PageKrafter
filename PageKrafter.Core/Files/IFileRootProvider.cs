namespace PageKrafter.Files;

/// <summary>
/// Chooses the folder the file picker and rich text editor open on. The host owns where files live, so
/// a site that keeps each tenant's uploads apart registers an implementation (scoped is fine) that
/// returns the current tenant's folder. The value is passed to the file manager as its root, and the
/// editor cannot browse above it. Register it before <c>AddPageKrafter</c>.
/// </summary>
public interface IFileRootProvider
{
    /// <summary>A folder under the host's upload directory, such as <c>Tenant_3</c>. Null opens the upload directory itself.</summary>
    string? GetFileRoot();
}

/// <summary>The default: the picker opens whatever the host's file manager is configured to show.</summary>
public sealed class DefaultFileRootProvider : IFileRootProvider
{
    /// <inheritdoc/>
    public string? GetFileRoot() => null;
}
