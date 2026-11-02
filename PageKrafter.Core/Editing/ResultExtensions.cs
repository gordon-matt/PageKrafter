namespace PageKrafter.Editing;

public static class ResultExtensions
{
    public static bool Succeeded(this IResult result) =>
        result.Status is ResultStatus.Ok or ResultStatus.Created or ResultStatus.NoContent;

    public static string ErrorMessage(this IResult result)
    {
        if (result.ValidationErrors?.FirstOrDefault() is { } validation)
        {
            return validation.ErrorMessage;
        }

        if (result.Errors?.FirstOrDefault() is { Length: > 0 } error)
        {
            return error;
        }

        return result.Status switch
        {
            ResultStatus.NotFound => "That item no longer exists.",
            ResultStatus.Forbidden => "That can't be changed.",
            _ => "Something went wrong.",
        };
    }
}
