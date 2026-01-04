using System.Text.Json;

namespace MozaHotkey.StreamDeck;

/// <summary>
/// Global plugin settings for increment values.
/// Persisted to JSON file in plugin directory.
/// </summary>
public class PluginSettings
{
    private static readonly string SettingsPath = Path.Combine(
        AppContext.BaseDirectory,
        "pluginSettings.json");

    private static PluginSettings? _instance;
    private static readonly object _lock = new();

    public static PluginSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= Load();
                }
            }
            return _instance;
        }
    }

    // Default increment values
    public int FfbIncrement { get; set; } = 5;
    public int RotationIncrement { get; set; } = 90;
    public int DampingIncrement { get; set; } = 5;
    public int RoadSensitivityIncrement { get; set; } = 1;
    public int MaxTorqueIncrement { get; set; } = 5;

    private static PluginSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<PluginSettings>(json) ?? new PluginSettings();
            }
        }
        catch
        {
            // Ignore load errors, use defaults
        }
        return new PluginSettings();
    }

    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    /// <summary>
    /// Reloads settings from disk.
    /// </summary>
    public static void Reload()
    {
        lock (_lock)
        {
            _instance = Load();
        }
    }
}
