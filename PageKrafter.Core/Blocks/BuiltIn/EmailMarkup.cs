namespace PageKrafter.Blocks.BuiltIn;

internal static class EmailMarkup
{
    public const string Table = "role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"";

    /// <summary>Padding and background must sit on the cell in email, because many clients ignore them on tables.</summary>
    public static readonly IReadOnlyCollection<string> CellStyles = [CommonStyles.Padding, CommonStyles.Background];

    public static string CellStyle(BlockRenderContext context, string defaultPadding = "") =>
        Css.Declarations(
            ("padding", context.Node.Get(CommonStyles.Padding) is { Length: > 0 } padding ? padding : defaultPadding),
            ("background-color", context.Node.Get(CommonStyles.Background)));
}
