namespace PageKrafter.Documents;

[Flags]
public enum DocumentMode
{
    Web = 1,
    Email = 2,
    All = Web | Email,
}