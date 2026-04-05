using BarRaider.SdTools;
using MozaStreamDeck.Core.Profiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaStreamDeck.Plugin.Actions;

[PluginActionId("com.dbce.moza-streamdeck.applypreset")]
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

        // Refresh preset list whenever the Property Inspector opens,
        // so newly created presets appear without restarting the plugin.
        Connection.OnPropertyInspectorDidAppear += (_, _) => RefreshPresetList();
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

            Logger.Instance.LogMessage(TracingLevel.INFO, $"ApplyPreset: loading preset from '{settings.PresetPath}'");
            var preset = PresetProfile.LoadFromFile(settings.PresetPath);
            if (preset == null)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"ApplyPreset: failed to load preset from '{settings.PresetPath}'");
                Connection.SetTitleAsync("Error");
                Connection.ShowAlert();
                return;
            }

            Logger.Instance.LogMessage(TracingLevel.INFO, $"ApplyPreset: loaded '{preset.Name}' with {preset.DeviceParams.Count} params: {string.Join(", ", preset.DeviceParams.Keys)}");
            if (preset.DeviceParams.TryGetValue("maximumSteeringAngle", out var angle))
                Logger.Instance.LogMessage(TracingLevel.INFO, $"ApplyPreset: maximumSteeringAngle={angle}");

            if (!MozaDeviceManager.Instance.EnsureInitialized())
            {
                Connection.ShowAlert();
                return;
            }
            var device = MozaDeviceManager.Instance.Device;

            // Log rotation before preset apply
            try
            {
                var (hwBefore, gameBefore) = device.GetWheelRotation();
                Logger.Instance.LogMessage(TracingLevel.INFO, $"ApplyPreset: rotation before=hw:{hwBefore}/game:{gameBefore}");
            }
            catch { }

            var (applied, failed, errors) = device.ApplyPreset(preset);

            // Log rotation after preset apply
            try
            {
                var (hwAfter, gameAfter) = device.GetWheelRotation();
                Logger.Instance.LogMessage(TracingLevel.INFO, $"ApplyPreset: rotation after=hw:{hwAfter}/game:{gameAfter}");
            }
            catch { }

            Logger.Instance.LogMessage(TracingLevel.INFO, $"ApplyPreset: '{preset.Name}' result: {applied} applied, {failed} failed");
            foreach (var error in errors)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Preset '{preset.Name}': {error}");
            }

            var name = preset.Name.Length > 12 ? preset.Name[..12] : preset.Name;
            if (applied > 0)
            {
                // Tell RotationAction the exact value we just set, so it doesn't
                // read back a stale value from the SDK and display the old rotation.
                if (preset.DeviceParams.TryGetValue("maximumSteeringAngle", out var angleVal))
                {
                    var rotationValue = Convert.ToInt32(angleVal) * 2;
                    MozaDeviceManager.SetRotationOverride(rotationValue);
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ApplyPreset: set rotation override to {rotationValue}");
                }

                var label = failed == 0 ? $"({applied})" : $"({applied}, {failed} skip)";
                Connection.SetTitleAsync($"{name}\n{label}");
                Connection.ShowOk();
                MozaDeviceManager.NotifyStateChanged();
            }
            else
            {
                Connection.SetTitleAsync($"{name}\n{failed} err");
                Connection.ShowAlert();
            }
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
        // Don't use Tools.AutoPopulateSettings — it uses Convert.ChangeType which
        // can't handle List<PresetEntry> and crashes the entire plugin.
        settings = payload.Settings.ToObject<PluginSettings>() ?? PluginSettings.CreateDefaultSettings();

        if (settings.PresetPath != previousPath)
        {
            UpdateTitle();
        }
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
