using System.Security.Cryptography;

namespace PageKrafter.Documents;

/// <summary>
/// Provides node identifier.
/// </summary>
public static class NodeId
{
    /// <summary>
    /// Gets the new.
    /// </summary>
    /// <returns>The new.</returns>
    public static string New() => "n" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(5));
}