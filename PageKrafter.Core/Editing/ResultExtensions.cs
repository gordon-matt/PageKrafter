namespace PageKrafter.Editing;

/// <summary>
/// Provides extension methods for results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Gets the succeeded.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns>true when the condition is met; otherwise, false.</returns>
    public static bool Succeeded(this IResult result) =>
        result.Status is ResultStatus.Ok or ResultStatus.Created or ResultStatus.NoContent;

    /// <summary>
    /// Gets the error message.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns>The error message.</returns>
    public static string ErrorMessage(this IResult result) => result.ValidationErrors?.FirstOrDefault() is { } validation
            ? validation.ErrorMessage
            : result.Errors?.FirstOrDefault() is { Length: > 0 } error
            ? error
            : result.Status switch
            {
                ResultStatus.NotFound => "That item no longer exists.",
                ResultStatus.Forbidden => "That can't be changed.",
                _ => "Something went wrong.",
            };
}