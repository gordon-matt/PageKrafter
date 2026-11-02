using System.Security.Cryptography;

namespace PageKrafter.Documents;

public static class NodeId
{
    public static string New() => "n" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(5));
}
