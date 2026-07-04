using System.Text.Json.Serialization;

namespace Trapezoid;

[Flags]
internal enum HotkeyModifiers : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008
}

internal sealed class Hotkey : IEquatable<Hotkey>
{
    public HotkeyModifiers Modifiers { get; set; }
    public Keys Key { get; set; }

    public Hotkey()
    {
    }

    public Hotkey(HotkeyModifiers modifiers, Keys key)
    {
        Modifiers = modifiers;
        Key = key;
    }

    [JsonIgnore]
    public uint NativeModifiers => (uint)Modifiers;

    [JsonIgnore]
    public uint NativeKey => (uint)Key;

    public bool Equals(Hotkey? other)
    {
        return other is not null && Modifiers == other.Modifiers && Key == other.Key;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Hotkey);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Modifiers, Key);
    }

    public override string ToString()
    {
        var parts = new List<string>();
        if (Modifiers.HasFlag(HotkeyModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (Modifiers.HasFlag(HotkeyModifiers.Win))
        {
            parts.Add("Win");
        }

        parts.Add(KeyName(Key));
        return string.Join(" + ", parts);
    }

    private static string KeyName(Keys key)
    {
        return key switch
        {
            Keys.Return => "Enter",
            Keys.Escape => "Esc",
            Keys.Space => "Space",
            Keys.Back => "Backspace",
            Keys.Delete => "Delete",
            Keys.Left => "Left",
            Keys.Right => "Right",
            Keys.Up => "Up",
            Keys.Down => "Down",
            Keys.OemMinus => "-",
            Keys.Oemplus => "=",
            Keys.Oemcomma => ",",
            Keys.OemPeriod => ".",
            Keys.OemQuestion => "/",
            Keys.OemPipe => "\\",
            Keys.OemOpenBrackets => "[",
            Keys.OemCloseBrackets => "]",
            Keys.OemSemicolon => ";",
            Keys.OemQuotes => "'",
            Keys.Oemtilde => "`",
            _ => key.ToString()
        };
    }
}
