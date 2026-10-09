namespace Trapezoid;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        bool background = args.Contains("--background", StringComparer.OrdinalIgnoreCase);
        using var instance = new Mutex(true, @"Local\Trapezoid.WindowManager.Instance", out bool created);
        if (!created)
        {
            if (!background)
            {
                try { using var show = EventWaitHandle.OpenExisting(@"Local\Trapezoid.WindowManager.Show"); show.Set(); }
                catch (WaitHandleCannotBeOpenedException) { }
            }
            return;
        }
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TrapezoidApplicationContext(background));
    }
}
