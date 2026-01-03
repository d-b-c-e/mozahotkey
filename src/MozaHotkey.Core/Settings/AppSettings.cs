using System.Text.Json;
using System.Text.Json.Serialization;

namespace MozaHotkey.Core.Settings;

/// <summary>
/// Application settings including hotkey bindings.
/// </summary>
public class AppSettings
{
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MozaHotkey");

    private static readonly string SettingsPath = Path.Combine(SettingsFolder, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public bool StartWithWindows { get; set; } = false;
    public bool StartMinimized { get; set; } = false;
    public bool ShowNotifications { get; set; } = true;
    public List<HotkeyBinding> Bindings { get; set; } = new();

    /// <summary>
    /// Gets or creates a binding for an action.
    /// </summary>
    public HotkeyBinding GetOrCreateBinding(string actionId)
    {
        var binding = Bindings.FirstOrDefault(b => b.ActionId == actionId);
        if (binding == null)
        {
            binding = new HotkeyBinding { ActionId = actionId };
            Bindings.Add(binding);
        }
        return binding;
    }

    /// <summary>
    /// Loads settings from disk.
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch
        {
            // If loading fails, return defaults
        }

        return new AppSettings();
    }

    /// <summary>
    /// Saves settings to disk.
    /// </summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsFolder);
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Ignore save errors for now
        }
    }

    /// <summary>
    /// Gets the settings folder path.
    /// </summary>
    public static string GetSettingsFolder() => SettingsFolder;
}
