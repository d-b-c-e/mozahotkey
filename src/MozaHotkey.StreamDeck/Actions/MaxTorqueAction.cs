using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.maxtorque")]
public class MaxTorqueAction : KeyAndEncoderBase
{
    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "direction")]
        public string Direction { get; set; } = "increase";
    }

    private PluginSettings settings;

    public MaxTorqueAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
                var currentValue = MozaDeviceManager.Instance.Device.GetMaxTorque();
                await Connection.SetTitleAsync($"TORQ\n{currentValue}%");
            }
            else
            {
                await Connection.SetTitleAsync("TORQ\nN/C");
            }
        }
        catch { await Connection.SetTitleAsync("TORQ\nN/C"); }
    }

    public override void KeyPressed(KeyPayload payload)
    {
        var increment = MozaHotkey.StreamDeck.PluginSettings.Instance.MaxTorqueIncrement;
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var newValue = settings.Direction == "decrease"
                ? device.AdjustMaxTorque(-increment)
                : device.AdjustMaxTorque(increment);
            Connection.SetTitleAsync($"TORQ\n{newValue}%");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("TORQ\nError");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"MaxTorque error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void DialRotate(DialRotatePayload payload)
    {
        var increment = MozaHotkey.StreamDeck.PluginSettings.Instance.MaxTorqueIncrement;
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var newValue = device.AdjustMaxTorque(payload.Ticks * increment);
            Connection.SetTitleAsync($"TORQ\n{newValue}%");
            // Max torque is 50-100, scale to 0-100
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{newValue}%" },
                { "indicator", ((newValue - 50) * 2).ToString() }
            });
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"MaxTorque dial error: {ex.Message}");
        }
    }

    public override void DialDown(DialPayload payload)
    {
        try
        {
            var currentValue = MozaDeviceManager.Instance.Device.GetMaxTorque();
            Connection.SetTitleAsync($"TORQ\n{currentValue}%");
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
