namespace PageKrafter.Editing;

/// <summary>Reads the status and the first message from an editor result.</summary>
public static class ResultExtensions
{
    /// <summary>Whether the status is OK, Created, or NoContent.</summary>
    /// <param name="result">The result.</param>
    /// <returns><see langword="true"/> for those three statuses.</returns>
    public static bool Succeeded(this IResult result) =>
        result.Status is ResultStatus.Ok or ResultStatus.Created or ResultStatus.NoContent;

    /// <summary>
    /// The first validation message, or the first error string. A not-found result says "That item no longer exists."
    /// A forbidden result says "That can't be changed." Anything else says "Something went wrong."
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns>A message an editor can show.</returns>
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