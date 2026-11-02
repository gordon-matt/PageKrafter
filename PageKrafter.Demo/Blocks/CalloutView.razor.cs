namespace PageKrafter.Demo.Blocks;

public partial class CalloutView
{
    private string Style
    {
        get
        {
            var tone = CalloutBlock.ToneFor(Value("tone"));
            return $"display:block;padding:16px 18px;border-left:4px solid {tone.Border};border-radius:6px;background:{tone.Background}";
        }
    }
}
