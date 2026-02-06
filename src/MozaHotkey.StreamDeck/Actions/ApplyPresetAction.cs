using BarRaider.SdTools;
using MozaHotkey.Core.Profiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.applypreset")]
public class ApplyPresetAction : KeypadBase
{
    private class PresetEntry
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = "";

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; } = "";

        [JsonProperty(PropertyName = "devices")]
        public string Devices { get; set; } = "";
    }

    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "presetPath")]
        public string PresetPath { get; set; } = "";

        [JsonProperty(PropertyName = "availablePresets")]
        public List<PresetEntry> AvailablePresets { get; set; } = new();
    }

    private PluginSettings settings;

    public ApplyPresetAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        if (payload.Settings == null || payload.Settings.Count == 0)
        {
            settings = PluginSettings.CreateDefaultSettings();
        }
        else
        {
            settings = payload.Settings.ToObject<PluginSettings>() ?? PluginSettings.CreateDefaultSettings();
        }
        RefreshPresetList();
        UpdateTitle();
    }

    private void RefreshPresetList()
    {
        try
        {
            var presets = PresetManager.GetMotorPresets();
            settings.AvailablePresets = presets.Select(p => new PresetEntry
            {
                Name = p.Name,
                Path = p.FilePath,
                Devices = string.Join(", ", p.Devices)
            }).ToList();
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Error enumerating presets: {ex.Message}");
            settings.AvailablePresets = new List<PresetEntry>();
        }
        Connection.SetSettingsAsync(JObject.FromObject(settings));
    }

    private void UpdateTitle()
    {
        if (string.IsNullOrEmpty(settings.PresetPath))
        {
            Connection.SetTitleAsync("No\nPreset");
            return;
        }

        var preset = PresetProfile.LoadFromFile(settings.PresetPath);
        if (preset != null)
        {
            var name = preset.Name.Length > 12 ? preset.Name[..12] : preset.Name;
            Connection.SetTitleAsync(name);
        }
        else
        {
            Connection.SetTitleAsync("No\nPreset");
        }
    }

    public override void KeyPressed(KeyPayload payload)
    {
        try
        {
            if (string.IsNullOrEmpty(settings.PresetPath))
            {
                Connection.SetTitleAsync("No\nPreset");
                Connection.ShowAlert();
                return;
            }

            var preset = PresetProfile.LoadFromFile(settings.PresetPath);
            if (preset == null)
            {
                Connection.SetTitleAsync("Error");
                Connection.ShowAlert();
                return;
            }

            var device = MozaDeviceManager.Instance.Device;
            var count = device.ApplyPreset(preset);

            var name = preset.Name.Length > 12 ? preset.Name[..12] : preset.Name;
            Connection.SetTitleAsync($"{name}\n({count})");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Apply preset error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }
    public override void OnTick() { }
    public override void Dispose() { }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        var previousPath = settings.PresetPath;
        Tools.AutoPopulateSettings(settings, payload.Settings);

        // Only refresh presets list if the path changed (user selected a preset)
        // Avoid re-enumerating when we just pushed settings ourselves
        if (settings.PresetPath != previousPath)
        {
            UpdateTitle();
        }
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
