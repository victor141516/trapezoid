using System.Diagnostics;
using System.Media;

namespace Trapezoid;

internal sealed class WindowManager
{
    private readonly int _currentProcessId = Process.GetCurrentProcess().Id;
    private readonly Dictionary<IntPtr, Rectangle> _restoreRects = new();
    private readonly Dictionary<IntPtr, WindowActionState> _lastActions = new();

    public void Execute(WindowActionId action, AppSettings settings)
    {
        var hwnd = GetTargetWindow();
        if (hwnd == IntPtr.Zero)
        {
            SystemSounds.Beep.Play();
            return;
        }

        if (action == WindowActionId.Restore)
        {
            Restore(hwnd);
            return;
        }

        if (!NativeMethods.GetWindowRect(hwnd, out var nativeRect))
        {
            SystemSounds.Beep.Play();
            return;
        }

        var current = nativeRect.ToRectangle();
        var repeatedExecutionCount = 0;
        var movedOutsideTrapezoid = false;
        if (_lastActions.TryGetValue(hwnd, out var lastAction))
        {
            movedOutsideTrapezoid = !RectanglesClose(current, lastAction.Rect);
            if (movedOutsideTrapezoid)
            {
                _lastActions.Remove(hwnd);
            }
            else if (lastAction.Action == action)
            {
                repeatedExecutionCount = lastAction.Count;
            }
        }

        if (!_restoreRects.ContainsKey(hwnd) || movedOutsideTrapezoid)
        {
            _restoreRects[hwnd] = current;
        }

        var screen = Screen.FromHandle(hwnd);
        var target = WindowLayoutCalculator.Calculate(action, current, screen, settings, repeatedExecutionCount);
        if (target.IsEmpty)
        {
            SystemSounds.Beep.Play();
            return;
        }

        MoveWindow(hwnd, target);
        _lastActions[hwnd] = new WindowActionState(action, target, repeatedExecutionCount + 1);
    }

    private IntPtr GetTargetWindow()
    {
        var hwnd = NativeMethods.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        hwnd = NativeMethods.GetRootWindow(hwnd);
        if (hwnd == IntPtr.Zero || hwnd == NativeMethods.GetShellWindow() || !NativeMethods.IsWindowVisible(hwnd))
        {
            return IntPtr.Zero;
        }

        NativeMethods.GetWindowThreadProcessId(hwnd, out var processId);
        return processId == _currentProcessId ? IntPtr.Zero : hwnd;
    }

    private void Restore(IntPtr hwnd)
    {
        if (_restoreRects.TryGetValue(hwnd, out var rect))
        {
            MoveWindow(hwnd, rect);
            _restoreRects.Remove(hwnd);
            _lastActions.Remove(hwnd);
            return;
        }

        if (NativeMethods.IsIconic(hwnd) || NativeMethods.IsZoomed(hwnd))
        {
            NativeMethods.ShowWindow(hwnd, NativeMethods.SwRestore);
        }
    }

    private static void MoveWindow(IntPtr hwnd, Rectangle target)
    {
        if (NativeMethods.IsIconic(hwnd) || NativeMethods.IsZoomed(hwnd))
        {
            NativeMethods.ShowWindow(hwnd, NativeMethods.SwRestore);
        }

        var moved = NativeMethods.SetWindowPos(
            hwnd,
            IntPtr.Zero,
            target.Left,
            target.Top,
            target.Width,
            target.Height,
            NativeMethods.SwpNoZOrder | NativeMethods.SwpShowWindow);

        if (!moved)
        {
            SystemSounds.Beep.Play();
        }
    }

    private static bool RectanglesClose(Rectangle first, Rectangle second)
    {
        const int tolerance = 6;
        return Math.Abs(first.Left - second.Left) <= tolerance
            && Math.Abs(first.Top - second.Top) <= tolerance
            && Math.Abs(first.Width - second.Width) <= tolerance
            && Math.Abs(first.Height - second.Height) <= tolerance;
    }
}

internal sealed record WindowActionState(WindowActionId Action, Rectangle Rect, int Count);
