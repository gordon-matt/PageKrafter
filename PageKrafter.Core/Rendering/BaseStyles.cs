namespace PageKrafter.Rendering;

/// <summary>
/// The few rules that inline styles can't express: column stacking and per-device visibility.
/// Everything else is inline so exported HTML looks right without any stylesheet.
/// </summary>
public static class BaseStyles
{
    public const int MobileBreakpoint = 768;

    public const string Web =
        ".pk-page{box-sizing:border-box}" +
        ".pk-page *,.pk-page *::before,.pk-page *::after{box-sizing:inherit}" +
        ".pk-section{position:relative;background-size:cover;background-position:center}" +
        ".pk-container{width:100%;margin-left:auto;margin-right:auto}" +
        ".pk-columns{display:flex;flex-wrap:wrap}" +
        ".pk-column{min-width:0}" +
        ".pk-img{max-width:100%;height:auto;vertical-align:middle}" +
        ".pk-image{margin:0}" +
        ".pk-btn{display:inline-block;text-decoration:none;line-height:1.2}" +
        ".pk-video{position:relative;height:0;overflow:hidden}" +
        ".pk-video iframe,.pk-video video{position:absolute;inset:0;width:100%;height:100%;border:0}" +
        ".pk-text>:first-child{margin-top:0}.pk-text>:last-child{margin-bottom:0}" +
        "@media (max-width:767.98px){" +
        ".pk-columns.pk-stack{flex-direction:column}" +
        ".pk-columns.pk-stack>.pk-column{flex:1 1 auto!important;width:100%}" +
        ".pk-hide-mobile{display:none!important}}" +
        "@media (min-width:768px){.pk-hide-desktop{display:none!important}}";

    public const string Email =
        "body{margin:0;padding:0;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%}" +
        "table,td{border-collapse:collapse;mso-table-lspace:0pt;mso-table-rspace:0pt}" +
        "img{border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic}" +
        ".pk-text p{margin:0 0 12px}.pk-text p:last-child{margin-bottom:0}" +
        "@media only screen and (max-width:620px){" +
        ".pk-email-body{width:100%!important}" +
        ".pk-email-col{display:block!important;width:100%!important;padding-left:0!important;padding-right:0!important;padding-bottom:16px!important}}";

    public static string For(DocumentMode mode) => mode == DocumentMode.Email ? Email : Web;
}