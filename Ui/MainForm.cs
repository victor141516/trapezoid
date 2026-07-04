namespace Trapezoid;

internal sealed class MainForm : Form
{
    private readonly SettingsStore _settingsStore;
    private readonly HotkeyService _hotkeyService;
    private readonly Func<IReadOnlyList<HotkeyRegistrationResult>> _refreshHotkeys;
    private readonly Dictionary<WindowActionId, ShortcutRecorderButton> _shortcutButtons = new();
    private readonly List<TableLayoutPanel> _shortcutGroupPanels = new();
    private readonly ToolStripStatusLabel _statusLabel = new();
    private FlowLayoutPanel? _shortcutsFlow;
    private CheckBox? _enabledCheckbox;
    private CheckBox? _launchAtLoginCheckbox;
    private NumericUpDown? _gapInput;
    private NumericUpDown? _resizeStepInput;

    public MainForm(
        SettingsStore settingsStore,
        HotkeyService hotkeyService,
        Func<IReadOnlyList<HotkeyRegistrationResult>> refreshHotkeys)
    {
        _settingsStore = settingsStore;
        _hotkeyService = hotkeyService;
        _refreshHotkeys = refreshHotkeys;

        Text = "Trapezoid";
        Icon = IconFactory.CreateIcon();
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(760, 560);
        ClientSize = new Size(860, 650);
        Font = new Font("Segoe UI", 9F);

        Controls.Add(BuildTabs());
        Controls.Add(new StatusStrip
        {
            Items = { _statusLabel },
            SizingGrip = false
        });
    }

    public void ReloadSettings()
    {
        var settings = _settingsStore.Settings;
        _enabledCheckbox?.SetCheckedSilently(settings.ShortcutsEnabled);
        _launchAtLoginCheckbox?.SetCheckedSilently(StartupRegistration.IsEnabled());

        if (_gapInput is not null)
        {
            _gapInput.Value = settings.GapSize;
        }

        if (_resizeStepInput is not null)
        {
            _resizeStepInput.Value = settings.ResizeStep;
        }

        foreach (var action in WindowActions.All)
        {
            if (_shortcutButtons.TryGetValue(action.Id, out var button))
            {
                settings.Shortcuts.TryGetValue(action.Id.ToString(), out var shortcut);
                button.SetShortcut(shortcut);
            }
        }

        UpdateHotkeyStatus(_hotkeyService.LastResults);
    }

    public void UpdateHotkeyStatus(IReadOnlyList<HotkeyRegistrationResult> results)
    {
        if (!_settingsStore.Settings.ShortcutsEnabled)
        {
            _statusLabel.Text = "Shortcuts disabled";
            return;
        }

        var active = results.Count(result => result.Success);
        var failed = results.Count - active;
        _statusLabel.Text = failed == 0
            ? $"{active} shortcuts active"
            : $"{active} shortcuts active, {failed} unavailable";
    }

    private TabControl BuildTabs()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Padding = new Point(14, 5)
        };

        tabs.TabPages.Add(BuildShortcutsPage());
        tabs.TabPages.Add(BuildSettingsPage());
        tabs.TabPages.Add(BuildAboutPage());
        return tabs;
    }

    private TabPage BuildShortcutsPage()
    {
        var page = new TabPage("Shortcuts") { Padding = new Padding(12) };
        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = SystemColors.Window,
            Padding = new Padding(6)
        };

        _shortcutsFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        foreach (var group in WindowActions.Groups)
        {
            _shortcutsFlow.Controls.Add(BuildShortcutGroup(group));
        }

        scroll.Controls.Add(_shortcutsFlow);
        scroll.Resize += (_, _) => UpdateShortcutColumns();
        page.HandleCreated += (_, _) => BeginInvoke((Action)UpdateShortcutColumns);
        page.Controls.Add(scroll);
        return page;
    }

    private TableLayoutPanel BuildShortcutGroup(IGrouping<string, WindowActionDefinition> group)
    {
        var table = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Margin = new Padding(0, 0, 12, 12),
            Padding = Padding.Empty,
            Width = 380
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.Controls.Add(BuildGroupHeader(group.Key));

        foreach (var action in group)
        {
            table.Controls.Add(BuildShortcutRow(action));
        }

        _shortcutGroupPanels.Add(table);
        return table;
    }

    private void UpdateShortcutColumns()
    {
        if (_shortcutsFlow?.Parent is not Panel scroll)
        {
            return;
        }

        var available = scroll.ClientSize.Width
            - scroll.Padding.Horizontal
            - SystemInformation.VerticalScrollBarWidth
            - 2;
        if (available <= 0)
        {
            return;
        }

        var columns = available >= 1180 ? 3 : available >= 720 ? 2 : 1;
        var gap = 12;
        var groupWidth = Math.Max(320, (available - gap * (columns - 1)) / columns);

        _shortcutsFlow.SuspendLayout();
        _shortcutsFlow.Width = available;
        foreach (var groupPanel in _shortcutGroupPanels)
        {
            groupPanel.Width = groupWidth;
            foreach (Control child in groupPanel.Controls)
            {
                child.Width = groupWidth;
            }
        }

        _shortcutsFlow.ResumeLayout(true);
    }

    private static Label BuildGroupHeader(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Height = 32,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.BottomLeft,
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 55, 55),
            Padding = new Padding(0, 8, 0, 4),
            Margin = new Padding(0, 8, 0, 2)
        };
    }

    private Control BuildShortcutRow(WindowActionDefinition action)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = false,
            Height = 31,
            Dock = DockStyle.Top,
            ColumnCount = 3,
            Margin = new Padding(0, 0, 0, 3),
            Padding = new Padding(0, 1, 0, 1)
        };

        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142));

        var label = new Label
        {
            Text = action.DisplayName,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        var button = new ShortcutRecorderButton { Dock = DockStyle.Fill };
        _settingsStore.Settings.Shortcuts.TryGetValue(action.Id.ToString(), out var shortcut);
        button.SetShortcut(shortcut);
        button.ShortcutChanged += (_, newShortcut) =>
        {
            _settingsStore.Settings.Shortcuts[action.Id.ToString()] = newShortcut;
            _settingsStore.Save();
        };
        button.RecordingChanged += (_, recording) =>
        {
            if (recording)
            {
                _hotkeyService.Suspend();
                UpdateHotkeyStatus(_hotkeyService.LastResults);
            }
            else
            {
                UpdateHotkeyStatus(_refreshHotkeys());
            }
        };
        _shortcutButtons[action.Id] = button;

        row.Controls.Add(new ActionGlyph(action), 0, 0);
        row.Controls.Add(label, 1, 0);
        row.Controls.Add(button, 2, 0);
        return row;
    }

    private TabPage BuildSettingsPage()
    {
        var page = new TabPage("Settings") { Padding = new Padding(18) };
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(4)
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _enabledCheckbox = new CheckBox { Text = "Enable shortcuts", AutoSize = true };
        _enabledCheckbox.CheckedChanged += (_, _) =>
        {
            _settingsStore.Settings.ShortcutsEnabled = _enabledCheckbox.Checked;
            _settingsStore.Save();
            UpdateHotkeyStatus(_refreshHotkeys());
        };
        table.Controls.Add(_enabledCheckbox, 0, 0);
        table.SetColumnSpan(_enabledCheckbox, 2);

        _launchAtLoginCheckbox = new CheckBox { Text = "Launch at login", AutoSize = true };
        _launchAtLoginCheckbox.CheckedChanged += (_, _) =>
        {
            StartupRegistration.SetEnabled(_launchAtLoginCheckbox.Checked);
            _settingsStore.Settings.LaunchAtLogin = _launchAtLoginCheckbox.Checked;
            _settingsStore.Save();
        };
        table.Controls.Add(_launchAtLoginCheckbox, 0, 1);
        table.SetColumnSpan(_launchAtLoginCheckbox, 2);

        _gapInput = AddNumericSetting(table, 2, "Gap size", 0, 64, _settingsStore.Settings.GapSize);
        _gapInput.ValueChanged += (_, _) =>
        {
            _settingsStore.Settings.GapSize = (int)_gapInput.Value;
            _settingsStore.Save();
        };

        _resizeStepInput = AddNumericSetting(table, 3, "Resize step", 10, 240, _settingsStore.Settings.ResizeStep);
        _resizeStepInput.ValueChanged += (_, _) =>
        {
            _settingsStore.Settings.ResizeStep = (int)_resizeStepInput.Value;
            _settingsStore.Save();
        };

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 18, 0, 0)
        };
        buttons.Controls.Add(MakeButton("Restore Defaults", RestoreDefaults));
        buttons.Controls.Add(MakeButton("Import JSON", ImportConfig));
        buttons.Controls.Add(MakeButton("Export JSON", ExportConfig));

        table.Controls.Add(buttons, 0, 4);
        table.SetColumnSpan(buttons, 2);
        page.Controls.Add(table);
        return page;
    }

    private NumericUpDown AddNumericSetting(TableLayoutPanel table, int row, string labelText, int min, int max, int value)
    {
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Height = 32,
            Margin = new Padding(0, 12, 16, 0)
        };
        var input = new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            Value = Math.Clamp(value, min, max),
            Width = 90,
            Margin = new Padding(0, 12, 0, 0)
        };

        table.Controls.Add(label, 0, row);
        table.Controls.Add(input, 1, row);
        return input;
    }

    private static Button MakeButton(string text, EventHandler click)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 30,
            Margin = new Padding(0, 0, 8, 0)
        };
        button.Click += click;
        return button;
    }

    private void RestoreDefaults(object? sender, EventArgs e)
    {
        _settingsStore.RestoreDefaultShortcuts();
        _settingsStore.Save();
        ReloadSettings();
        UpdateHotkeyStatus(_refreshHotkeys());
    }

    private void ImportConfig(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Import Trapezoid Settings"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _settingsStore.Import(dialog.FileName);
            ReloadSettings();
            UpdateHotkeyStatus(_refreshHotkeys());
        }
    }

    private void ExportConfig(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Export Trapezoid Settings",
            FileName = "TrapezoidConfig.json"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _settingsStore.Export(dialog.FileName);
        }
    }

    private static TabPage BuildAboutPage()
    {
        var page = new TabPage("About") { Padding = new Padding(24) };
        var title = new Label
        {
            Text = "Trapezoid",
            Dock = DockStyle.Top,
            Height = 38,
            Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold)
        };
        var body = new Label
        {
            Text = "A Windows-native Rectangle-style keyboard window manager.",
            Dock = DockStyle.Top,
            Height = 28
        };
        page.Controls.Add(body);
        page.Controls.Add(title);
        return page;
    }
}

internal static class CheckBoxExtensions
{
    public static void SetCheckedSilently(this CheckBox checkBox, bool value)
    {
        if (checkBox.Checked != value)
        {
            checkBox.Checked = value;
        }
    }
}
