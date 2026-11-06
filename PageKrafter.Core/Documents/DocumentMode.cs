namespace PageKrafter.Documents;

/// <summary>
/// Specifies the document mode.
/// </summary>
[Flags]
public enum DocumentMode
{
    /// <summary>
    /// The web value.
    /// </summary>
    Web = 1,
    /// <summary>
    /// The email value.
    /// </summary>
    Email = 2,
    /// <summary>
    /// The all value.
    /// </summary>
    All = Web | Email,
}