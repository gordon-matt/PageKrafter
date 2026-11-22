using System.Security.Cryptography;

namespace PageKrafter.Documents;

/// <summary>Creates the ids stored on blocks. The page root keeps the fixed id <c>root</c>.</summary>
public static class NodeId
{
    /// <summary>A new id: <c>n</c> followed by ten lowercase hexadecimal characters.</summary>
    /// <returns>The id.</returns>
    public static string New() => "n" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(5));
}