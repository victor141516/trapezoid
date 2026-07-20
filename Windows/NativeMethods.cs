using System.Runtime.InteropServices;

namespace Trapezoid;

internal static class NativeMethods
{
    public const int SwRestore = 9;
    public const uint SwpNoZOrder = 0x0004;
    public const uint SwpShowWindow = 0x0040;
    public const int DwmwaExtendedFrameBounds = 9;
    public const int DwmwaWindowCornerPreference = 33;
    public const int DwmwcpDoNotRound = 1;
    private const int GaRoot = 2;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

    [DllImport("user32.dll")]
    public static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsZoomed(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(
        IntPtr hWnd,
        int dwAttribute,
        out NativeRect pvAttribute,
        int cbAttribute);

    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(
        IntPtr hWnd,
        int dwAttribute,
        ref int pvAttribute,
        int cbAttribute);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    public static IntPtr GetRootWindow(IntPtr hWnd)
    {
        return GetAncestor(hWnd, GaRoot);
    }

    public static bool IsKeyDown(Keys key)
    {
        return (GetKeyState((int)key) & 0x8000) != 0;
    }

    public static bool GetVisibleWindowRect(IntPtr hWnd, out NativeRect rect)
    {
        var result = DwmGetWindowAttribute(
            hWnd,
            DwmwaExtendedFrameBounds,
            out rect,
            Marshal.SizeOf<NativeRect>());

        if (result == 0 && rect.Right > rect.Left && rect.Bottom > rect.Top)
        {
            return true;
        }

        return GetWindowRect(hWnd, out rect);
    }

    public static void DisableRoundedCorners(IntPtr hWnd)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            return;
        }

        var preference = DwmwcpDoNotRound;
        _ = DwmSetWindowAttribute(
            hWnd,
            DwmwaWindowCornerPreference,
            ref preference,
            sizeof(int));
    }
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct NativeRect
{
    public readonly int Left;
    public readonly int Top;
    public readonly int Right;
    public readonly int Bottom;

    public Rectangle ToRectangle()
    {
        return Rectangle.FromLTRB(Left, Top, Right, Bottom);
    }
}
