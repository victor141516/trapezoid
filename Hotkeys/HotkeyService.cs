using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Trapezoid;

internal sealed class HotkeyService : NativeWindow, IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModNoRepeat = 0x4000;
    private readonly Action<WindowActionId> _executeAction;
    private readonly Dictionary<int, IReadOnlyList<WindowActionId>> _actionsByNativeId = new();
    private readonly Dictionary<int, int> _cycleIndexByNativeId = new();
    private int _nextNativeId = 100;
    private bool _disposed;

    public HotkeyService(Action<WindowActionId> executeAction)
    {
        _executeAction = executeAction;
        CreateHandle(new CreateParams());
    }

    public IReadOnlyList<HotkeyRegistrationResult> LastResults { get; private set; } =
        Array.Empty<HotkeyRegistrationResult>();

    public IReadOnlyList<HotkeyRegistrationResult> Register(AppSettings settings)
    {
        UnregisterAll();

        if (!settings.ShortcutsEnabled)
        {
            LastResults = Array.Empty<HotkeyRegistrationResult>();
            return LastResults;
        }

        var grouped = new Dictionary<Hotkey, List<WindowActionId>>();
        foreach (var definition in WindowActions.All)
        {
            if (!settings.Shortcuts.TryGetValue(definition.Id.ToString(), out var shortcut) || shortcut is null)
            {
                continue;
            }

            if (!grouped.TryGetValue(shortcut, out var actions))
            {
                actions = new List<WindowActionId>();
                grouped[shortcut] = actions;
            }

            actions.Add(definition.Id);
        }

        var results = new List<HotkeyRegistrationResult>();
        foreach (var item in grouped)
        {
            var nativeId = _nextNativeId++;
            var modifiers = item.Key.NativeModifiers | ModNoRepeat;
            var registered = NativeMethods.RegisterHotKey(Handle, nativeId, modifiers, item.Key.NativeKey);
            var error = registered ? 0 : new Win32Exception(Marshal.GetLastWin32Error()).NativeErrorCode;

            if (registered)
            {
                _actionsByNativeId[nativeId] = item.Value;
                _cycleIndexByNativeId[nativeId] = 0;
            }

            results.Add(new HotkeyRegistrationResult(item.Key, item.Value, registered, error));
        }

        LastResults = results;
        return LastResults;
    }

    public void Suspend()
    {
        UnregisterAll();
        LastResults = Array.Empty<HotkeyRegistrationResult>();
    }

    private void UnregisterAll()
    {
        foreach (var nativeId in _actionsByNativeId.Keys.ToArray())
        {
            NativeMethods.UnregisterHotKey(Handle, nativeId);
        }

        _actionsByNativeId.Clear();
        _cycleIndexByNativeId.Clear();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey)
        {
            var nativeId = m.WParam.ToInt32();
            if (_actionsByNativeId.TryGetValue(nativeId, out var actions) && actions.Count > 0)
            {
                var index = _cycleIndexByNativeId.GetValueOrDefault(nativeId) % actions.Count;
                _cycleIndexByNativeId[nativeId] = (index + 1) % actions.Count;
                _executeAction(actions[index]);
            }

            return;
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        UnregisterAll();
        DestroyHandle();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

internal sealed record HotkeyRegistrationResult(
    Hotkey Shortcut,
    IReadOnlyList<WindowActionId> Actions,
    bool Success,
    int ErrorCode);
