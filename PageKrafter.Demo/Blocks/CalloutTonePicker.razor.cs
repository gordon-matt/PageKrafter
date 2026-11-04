namespace PageKrafter.Demo.Blocks;

public partial class CalloutTonePicker
{
    private string Current => CalloutBlock.ToneFor(Value).Key;
}