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

        if (!NativeMethods.GetVisibleWindowRect(hwnd, out var nativeRect))
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

        if (MoveWindow(hwnd, target, out var actual))
        {
            _lastActions[hwnd] = new WindowActionState(action, actual, repeatedExecutionCount + 1);
        }
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
            if (MoveWindow(hwnd, rect, out _))
            {
                _restoreRects.Remove(hwnd);
                _lastActions.Remove(hwnd);
            }

            return;
        }

        if (NativeMethods.IsIconic(hwnd) || NativeMethods.IsZoomed(hwnd))
        {
            NativeMethods.ShowWindow(hwnd, NativeMethods.SwRestore);
        }
    }

    private static bool MoveWindow(IntPtr hwnd, Rectangle target, out Rectangle actual)
    {
        actual = target;

        if (NativeMethods.IsIconic(hwnd) || NativeMethods.IsZoomed(hwnd))
        {
            NativeMethods.ShowWindow(hwnd, NativeMethods.SwRestore);
        }

        NativeMethods.DisableRoundedCorners(hwnd);

        if (!TryGetWindowBounds(hwnd, out var window, out var visible))
        {
            SystemSounds.Beep.Play();
            return false;
        }

        var requestedWindow = WindowBoundsForVisibleTarget(target, window, visible);
        var moved = SetWindowPos(hwnd, requestedWindow);
        if (!moved)
        {
            SystemSounds.Beep.Play();
            return false;
        }

        // Some windows update their non-client frame only after the first move,
        // especially when crossing displays with different DPI settings. Correct
        // once using the frame Windows actually rendered.
        if (TryGetWindowBounds(hwnd, out window, out visible) && visible != target)
        {
            var correction = Rectangle.FromLTRB(
                window.Left + target.Left - visible.Left,
                window.Top + target.Top - visible.Top,
                window.Right + target.Right - visible.Right,
                window.Bottom + target.Bottom - visible.Bottom);

            if (correction.Width > 0 && correction.Height > 0)
            {
                moved = SetWindowPos(hwnd, correction);
            }
        }

        if (NativeMethods.GetVisibleWindowRect(hwnd, out var actualRect))
        {
            actual = actualRect.ToRectangle();
        }

        if (!moved)
        {
            SystemSounds.Beep.Play();
        }

        return moved;
    }

    private static bool SetWindowPos(IntPtr hwnd, Rectangle target)
    {
        return NativeMethods.SetWindowPos(
            hwnd,
            IntPtr.Zero,
            target.Left,
            target.Top,
            target.Width,
            target.Height,
            NativeMethods.SwpNoZOrder | NativeMethods.SwpShowWindow);
    }

    private static bool TryGetWindowBounds(IntPtr hwnd, out Rectangle window, out Rectangle visible)
    {
        window = Rectangle.Empty;
        visible = Rectangle.Empty;

        if (!NativeMethods.GetWindowRect(hwnd, out var windowRect)
            || !NativeMethods.GetVisibleWindowRect(hwnd, out var visibleRect))
        {
            return false;
        }

        window = windowRect.ToRectangle();
        visible = visibleRect.ToRectangle();
        return true;
    }

    private static Rectangle WindowBoundsForVisibleTarget(
        Rectangle target,
        Rectangle window,
        Rectangle visible)
    {
        return Rectangle.FromLTRB(
            target.Left - (visible.Left - window.Left),
            target.Top - (visible.Top - window.Top),
            target.Right + (window.Right - visible.Right),
            target.Bottom + (window.Bottom - visible.Bottom));
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
