using System.Text.Json;

namespace Trapezoid;

internal sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        DirectoryPath = Path.Combine(appData, "Trapezoid");
        FilePath = Path.Combine(DirectoryPath, "settings.json");
        Settings = Load();
        EnsureShortcuts();
    }

    public string DirectoryPath { get; }
    public string FilePath { get; }
    public AppSettings Settings { get; private set; }

    public void Save()
    {
        Directory.CreateDirectory(DirectoryPath);
        var json = JsonSerializer.Serialize(Settings, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public void RestoreDefaultShortcuts()
    {
        Settings.Shortcuts.Clear();
        foreach (var action in WindowActions.All)
        {
            Settings.Shortcuts[action.Id.ToString()] = action.DefaultShortcut;
        }
    }

    public void Import(string path)
    {
        var json = File.ReadAllText(path);
        Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        EnsureShortcuts();
        Save();
    }

    public void Export(string path)
    {
        var json = JsonSerializer.Serialize(Settings, JsonOptions);
        File.WriteAllText(path, json);
    }

    private AppSettings Load()
    {
        if (!File.Exists(FilePath))
        {
            var fresh = new AppSettings();
            Settings = fresh;
            RestoreDefaultShortcuts();
            Save();
            return Settings;
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    private void EnsureShortcuts()
    {
        foreach (var action in WindowActions.All)
        {
            Settings.Shortcuts.TryAdd(action.Id.ToString(), action.DefaultShortcut);
        }

        Settings.GapSize = Math.Clamp(Settings.GapSize, 0, 64);
        Settings.ResizeStep = Math.Clamp(Settings.ResizeStep, 10, 240);
    }
}
