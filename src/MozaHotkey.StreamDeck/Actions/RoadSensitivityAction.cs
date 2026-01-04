using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.roadsensitivity")]
public class RoadSensitivityAction : KeyAndEncoderBase
{
    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "direction")]
        public string Direction { get; set; } = "increase";
    }

    private PluginSettings settings;

    public RoadSensitivityAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        settings = payload.Settings?.ToObject<PluginSettings>() ?? PluginSettings.CreateDefaultSettings();
        InitializeDisplay();
    }

    private async void InitializeDisplay()
    {
        try
        {
            if (MozaDeviceManager.Instance.TryInitialize())
            {
                var currentValue = MozaDeviceManager.Instance.Device.GetRoadSensitivity();
                await Connection.SetTitleAsync($"ROAD\n{currentValue}");
            }
            else
            {
                await Connection.SetTitleAsync("ROAD\nN/C");
            }
        }
        catch { await Connection.SetTitleAsync("ROAD\nN/C"); }
    }

    public override void KeyPressed(KeyPayload payload)
    {
        var increment = MozaHotkey.StreamDeck.PluginSettings.Instance.RoadSensitivityIncrement;
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var newValue = settings.Direction == "decrease"
                ? device.AdjustRoadSensitivity(-increment)
                : device.AdjustRoadSensitivity(increment);
            Connection.SetTitleAsync($"ROAD\n{newValue}");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("ROAD\nError");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"RoadSensitivity error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void DialRotate(DialRotatePayload payload)
    {
        var increment = MozaHotkey.StreamDeck.PluginSettings.Instance.RoadSensitivityIncrement;
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var newValue = device.AdjustRoadSensitivity(payload.Ticks * increment);
            Connection.SetTitleAsync($"ROAD\n{newValue}");
            // Road sensitivity is 0-10, scale to 0-100
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", newValue.ToString() },
                { "indicator", (newValue * 10).ToString() }
            });
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"RoadSensitivity dial error: {ex.Message}");
        }
    }

    public override void DialDown(DialPayload payload)
    {
        try
        {
            var currentValue = MozaDeviceManager.Instance.Device.GetRoadSensitivity();
            Connection.SetTitleAsync($"ROAD\n{currentValue}");
        }
        catch { }
    }

    public override void DialUp(DialPayload payload) { }
    public override void TouchPress(TouchpadPressPayload payload) { }
    public override void OnTick() { }
    public override void Dispose() { }
    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        Tools.AutoPopulateSettings(settings, payload.Settings);
        Connection.SetSettingsAsync(JObject.FromObject(settings));
    }
    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
