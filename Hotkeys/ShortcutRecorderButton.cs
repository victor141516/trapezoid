namespace Trapezoid;

internal sealed class ShortcutRecorderButton : Button
{
    private Hotkey? _shortcut;
    private bool _recording;
    private string _previousText = "";

    public event EventHandler<Hotkey?>? ShortcutChanged;
    public event EventHandler<bool>? RecordingChanged;

    public ShortcutRecorderButton()
    {
        AutoSize = false;
        Height = 27;
        TextAlign = ContentAlignment.MiddleCenter;
        FlatStyle = FlatStyle.System;
        TabStop = true;
        SetShortcut(null);
    }

    public void SetShortcut(Hotkey? shortcut)
    {
        _shortcut = shortcut;
        Text = shortcut?.ToString() ?? "None";
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        BeginRecording();
    }

    protected override bool IsInputKey(Keys keyData)
    {
        return _recording || base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!_recording)
        {
            base.OnKeyDown(e);
            return;
        }

        e.Handled = true;
        e.SuppressKeyPress = true;

        if (e.KeyCode == Keys.Escape)
        {
            EndRecording(false);
            return;
        }

        if (e.KeyCode is Keys.Back or Keys.Delete)
        {
            _shortcut = null;
            ShortcutChanged?.Invoke(this, null);
            EndRecording(true);
            return;
        }

        if (IsModifierOnly(e.KeyCode))
        {
            return;
        }

        var modifiers = HotkeyModifiers.None;
        if ((e.Modifiers & Keys.Control) == Keys.Control)
        {
            modifiers |= HotkeyModifiers.Control;
        }

        if ((e.Modifiers & Keys.Alt) == Keys.Alt)
        {
            modifiers |= HotkeyModifiers.Alt;
        }

        if ((e.Modifiers & Keys.Shift) == Keys.Shift)
        {
            modifiers |= HotkeyModifiers.Shift;
        }

        if (NativeMethods.IsKeyDown(Keys.LWin) || NativeMethods.IsKeyDown(Keys.RWin))
        {
            modifiers |= HotkeyModifiers.Win;
        }

        _shortcut = new Hotkey(modifiers, e.KeyCode);
        ShortcutChanged?.Invoke(this, _shortcut);
        EndRecording(true);
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        if (_recording)
        {
            EndRecording(false);
        }
    }

    private void BeginRecording()
    {
        if (_recording)
        {
            return;
        }

        _recording = true;
        _previousText = Text;
        Text = "Type shortcut";
        Capture = true;
        Focus();
        RecordingChanged?.Invoke(this, true);
    }

    private void EndRecording(bool changed)
    {
        Capture = false;
        _recording = false;
        Text = changed ? _shortcut?.ToString() ?? "None" : _previousText;
        RecordingChanged?.Invoke(this, false);
    }

    private static bool IsModifierOnly(Keys key)
    {
        return key is Keys.ControlKey
            or Keys.ShiftKey
            or Keys.Menu
            or Keys.LMenu
            or Keys.RMenu
            or Keys.LControlKey
            or Keys.RControlKey
            or Keys.LShiftKey
            or Keys.RShiftKey
            or Keys.LWin
            or Keys.RWin;
    }
}
