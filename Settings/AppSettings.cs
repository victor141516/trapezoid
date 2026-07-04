namespace Trapezoid;

internal sealed class AppSettings
{
    public bool ShortcutsEnabled { get; set; } = true;
    public bool LaunchAtLogin { get; set; }
    public int GapSize { get; set; }
    public int ResizeStep { get; set; } = 30;
    public Dictionary<string, Hotkey?> Shortcuts { get; set; } = new();
}
