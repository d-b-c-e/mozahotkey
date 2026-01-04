using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.settings")]
public class SettingsAction : KeypadBase
{
    private class ActionSettings
    {
        public static ActionSettings CreateDefaultSettings()
        {
            var ps = MozaHotkey.StreamDeck.PluginSettings.Instance;
            return new ActionSettings
            {
                FfbIncrement = ps.FfbIncrement,
                RotationIncrement = ps.RotationIncrement,
                DampingIncrement = ps.DampingIncrement,
                RoadSensitivityIncrement = ps.RoadSensitivityIncrement,
                MaxTorqueIncrement = ps.MaxTorqueIncrement
            };
        }

        [JsonProperty(PropertyName = "ffbIncrement")]
        public int FfbIncrement { get; set; } = 5;

        [JsonProperty(PropertyName = "rotationIncrement")]
        public int RotationIncrement { get; set; } = 90;

        [JsonProperty(PropertyName = "dampingIncrement")]
        public int DampingIncrement { get; set; } = 5;

        [JsonProperty(PropertyName = "roadSensitivityIncrement")]
        public int RoadSensitivityIncrement { get; set; } = 1;

        [JsonProperty(PropertyName = "maxTorqueIncrement")]
        public int MaxTorqueIncrement { get; set; } = 5;
    }

    private ActionSettings settings;

    public SettingsAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        if (payload.Settings == null || payload.Settings.Count == 0)
        {
            settings = ActionSettings.CreateDefaultSettings();
            SaveSettings();
        }
        else
        {
            settings = payload.Settings.ToObject<ActionSettings>() ?? ActionSettings.CreateDefaultSettings();
            ApplySettings();
        }

        Connection.SetTitleAsync("MOZA\nSettings");
    }

    public override void KeyPressed(KeyPayload payload)
    {
        // Show current settings summary
        var ps = MozaHotkey.StreamDeck.PluginSettings.Instance;
        Connection.SetTitleAsync($"FFB:{ps.FfbIncrement}\nROT:{ps.RotationIncrement}");
        Connection.ShowOk();
    }

    public override void KeyReleased(KeyPayload payload) { }
    public override void OnTick() { }
    public override void Dispose() { }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        Tools.AutoPopulateSettings(settings, payload.Settings);
        ApplySettings();
        SaveSettings();
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

    private void ApplySettings()
    {
        var ps = MozaHotkey.StreamDeck.PluginSettings.Instance;
        ps.FfbIncrement = settings.FfbIncrement;
        ps.RotationIncrement = settings.RotationIncrement;
        ps.DampingIncrement = settings.DampingIncrement;
        ps.RoadSensitivityIncrement = settings.RoadSensitivityIncrement;
        ps.MaxTorqueIncrement = settings.MaxTorqueIncrement;
        ps.Save();
    }

    private void SaveSettings()
    {
        Connection.SetSettingsAsync(JObject.FromObject(settings));
    }
}
