using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.damping")]
public class DampingAction : KeyAndEncoderBase
{
    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "direction")]
        public string Direction { get; set; } = "increase";
    }

    private PluginSettings settings;

    public DampingAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
                var currentValue = MozaDeviceManager.Instance.Device.GetDamping();
                await Connection.SetTitleAsync($"DAMP\n{currentValue}%");
            }
            else
            {
                await Connection.SetTitleAsync("DAMP\nN/C");
            }
        }
        catch { await Connection.SetTitleAsync("DAMP\nN/C"); }
    }

    public override void KeyPressed(KeyPayload payload)
    {
        var increment = MozaHotkey.StreamDeck.PluginSettings.Instance.DampingIncrement;
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var newValue = settings.Direction == "decrease"
                ? device.AdjustDamping(-increment)
                : device.AdjustDamping(increment);
            Connection.SetTitleAsync($"DAMP\n{newValue}%");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("DAMP\nError");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Damping error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void DialRotate(DialRotatePayload payload)
    {
        var increment = MozaHotkey.StreamDeck.PluginSettings.Instance.DampingIncrement;
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var newValue = device.AdjustDamping(payload.Ticks * increment);
            Connection.SetTitleAsync($"DAMP\n{newValue}%");
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{newValue}%" },
                { "indicator", newValue.ToString() }
            });
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Damping dial error: {ex.Message}");
        }
    }

    public override void DialDown(DialPayload payload)
    {
        try
        {
            var currentValue = MozaDeviceManager.Instance.Device.GetDamping();
            Connection.SetTitleAsync($"DAMP\n{currentValue}%");
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
