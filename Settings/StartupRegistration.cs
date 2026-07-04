using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.Win32;

namespace Trapezoid;

internal static class StartupRegistration
{
    private const string ShortcutName = "Trapezoid.lnk";
    private const string LegacyRunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string LegacyRunValueName = "Trapezoid";

    public static string ShortcutPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ShortcutName);

    public static bool IsEnabled()
    {
        return File.Exists(ShortcutPath);
    }

    public static void SetEnabled(bool enabled)
    {
        RemoveLegacyRegistryStartup();

        if (enabled)
        {
            CreateShortcut();
            return;
        }

        if (File.Exists(ShortcutPath))
        {
            File.Delete(ShortcutPath);
        }
    }

    private static void RemoveLegacyRegistryStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(LegacyRunKey, true);
        key?.DeleteValue(LegacyRunValueName, false);
    }

    private static void CreateShortcut()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ShortcutPath)!);

        var shellLinkType = Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046"))!;
        var shellLinkObject = Activator.CreateInstance(shellLinkType)!;
        var shellLink = (IShellLinkW)shellLinkObject;
        try
        {
            var executablePath = Application.ExecutablePath;
            shellLink.SetPath(executablePath);
            shellLink.SetWorkingDirectory(Path.GetDirectoryName(executablePath) ?? AppContext.BaseDirectory);
            shellLink.SetDescription("Start Trapezoid at login");
            shellLink.SetIconLocation(executablePath, 0);

            var persistFile = (IPersistFile)shellLink;
            persistFile.Save(ShortcutPath, true);
        }
        finally
        {
            Marshal.FinalReleaseComObject(shellLinkObject);
        }
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}
