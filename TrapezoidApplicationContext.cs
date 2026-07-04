namespace Trapezoid;

internal sealed class TrapezoidApplicationContext : ApplicationContext
{
    private readonly SettingsStore _settingsStore = new();
    private readonly WindowManager _windowManager;
    private readonly HotkeyService _hotkeyService;
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _enabledMenuItem;
    private MainForm? _mainForm;
    private bool _exiting;

    public TrapezoidApplicationContext()
    {
        _windowManager = new WindowManager();
        _hotkeyService = new HotkeyService(action => _windowManager.Execute(action, _settingsStore.Settings));

        _enabledMenuItem = new ToolStripMenuItem("Shortcuts Enabled")
        {
            Checked = _settingsStore.Settings.ShortcutsEnabled,
            CheckOnClick = true
        };
        _enabledMenuItem.CheckedChanged += (_, _) =>
        {
            _settingsStore.Settings.ShortcutsEnabled = _enabledMenuItem.Checked;
            _settingsStore.Save();
            RefreshHotkeys();
            _mainForm?.ReloadSettings();
        };

        _notifyIcon = new NotifyIcon
        {
            Text = "Trapezoid",
            Icon = IconFactory.CreateIcon(),
            Visible = true,
            ContextMenuStrip = BuildTrayMenu()
        };
        _notifyIcon.DoubleClick += (_, _) => ShowMainForm();

        RefreshHotkeys();
        ShowMainForm();
    }

    private ContextMenuStrip BuildTrayMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open Trapezoid", null, (_, _) => ShowMainForm());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_enabledMenuItem);
        menu.Items.Add("Restore Rectangle Defaults", null, (_, _) =>
        {
            _settingsStore.RestoreDefaultShortcuts();
            _settingsStore.Save();
            RefreshHotkeys();
            _mainForm?.ReloadSettings();
        });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => ExitApplication());
        return menu;
    }

    private void ShowMainForm()
    {
        if (_mainForm is null || _mainForm.IsDisposed)
        {
            _mainForm = new MainForm(_settingsStore, _hotkeyService, RefreshHotkeys);
            _mainForm.FormClosing += (_, args) =>
            {
                if (_exiting || args.CloseReason != CloseReason.UserClosing)
                {
                    return;
                }

                args.Cancel = true;
                _mainForm.Hide();
            };
        }

        _mainForm.ReloadSettings();
        _mainForm.Show();
        _mainForm.WindowState = FormWindowState.Normal;
        _mainForm.Activate();
    }

    private IReadOnlyList<HotkeyRegistrationResult> RefreshHotkeys()
    {
        var results = _hotkeyService.Register(_settingsStore.Settings);
        _enabledMenuItem.Checked = _settingsStore.Settings.ShortcutsEnabled;
        _mainForm?.UpdateHotkeyStatus(results);
        return results;
    }

    private void ExitApplication()
    {
        _exiting = true;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _hotkeyService.Dispose();
        _mainForm?.Close();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notifyIcon.Dispose();
            _hotkeyService.Dispose();
        }

        base.Dispose(disposing);
    }
}
