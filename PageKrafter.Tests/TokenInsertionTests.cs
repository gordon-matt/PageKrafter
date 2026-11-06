namespace PageKrafter.Tests;

public class TokenInsertionTests
{
    [Theory]
    [InlineData("Hello", 5, 5, "{{ Name }}", "Hello{{ Name }}", 15)]
    [InlineData("Hello", 0, 0, "{{ Name }}", "{{ Name }}Hello", 10)]
    [InlineData("Hello", 1, 4, "i", "Hio", 2)]
    [InlineData("Hi", 20, 30, "!", "Hi!", 3)]
    [InlineData("Hi", -4, -1, "!", "!Hi", 1)]
    public void Insert_places_the_token_at_the_caret(string value, int start, int end, string token, string expected, int caret)
    {
        string updated = TokenInsertion.Insert(value, start, end, token, out int actualCaret);

        Assert.Equal(expected, updated);
        Assert.Equal(caret, actualCaret);
    }
}
