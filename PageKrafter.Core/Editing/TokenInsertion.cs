namespace PageKrafter.Editing;

/// <summary>Places a token into a field value. Offsets outside the text are pulled back in so a stale caret still lands on a character.</summary>
internal static class TokenInsertion
{
    public static string Insert(string? value, int start, int end, string? token, out int caret)
    {
        value ??= string.Empty;
        token ??= string.Empty;
        if (start < 0)
        {
            start = 0;
        }

        if (start > value.Length)
        {
            start = value.Length;
        }

        if (end < start)
        {
            end = start;
        }

        if (end > value.Length)
        {
            end = value.Length;
        }

        caret = start + token.Length;
        return string.Concat(value.AsSpan(0, start), token, value.AsSpan(end));
    }
}
