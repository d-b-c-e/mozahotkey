using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaHotkey.StreamDeck.Actions;

/// <summary>
/// Stream Deck action for adjusting wheel rotation.
/// Supports both buttons and dials.
/// </summary>
[PluginActionId("com.mozahotkey.streamdeck.rotation")]
public class RotationAction : KeyAndEncoderBase
{
    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "direction")]
        public string Direction { get; set; } = "increase";
    }

    private PluginSettings settings;

    public RotationAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        if (payload.Settings == null || payload.Settings.Count == 0)
        {
            settings = PluginSettings.CreateDefaultSettings();
            SaveSettings();
        }
        else
        {
            settings = payload.Settings.ToObject<PluginSettings>() ?? PluginSettings.CreateDefaultSettings();
        }

        InitializeDisplay();
    }

    private async void InitializeDisplay()
    {
        try
        {
            if (MozaDeviceManager.Instance.TryInitialize())
            {
                var (_, currentValue) = MozaDeviceManager.Instance.Device.GetWheelRotation();
                await Connection.SetTitleAsync($"ROT\n{currentValue}°");
            }
            else
            {
                await Connection.SetTitleAsync("ROT\nN/C");
            }
        }
        catch
        {
            await Connection.SetTitleAsync("ROT\nN/C");
        }
    }

    public override void KeyPressed(KeyPayload payload)
    {
        var increment = MozaHotkey.StreamDeck.PluginSettings.Instance.RotationIncrement;

        try
        {
            var device = MozaDeviceManager.Instance.Device;
            int newValue;

            if (settings.Direction == "decrease")
            {
                newValue = device.AdjustWheelRotation(-increment);
            }
            else
            {
                newValue = device.AdjustWheelRotation(increment);
            }

            Connection.SetTitleAsync($"ROT\n{newValue}°");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("ROT\nError");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Rotation action error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void DialRotate(DialRotatePayload payload)
    {
        var increment = MozaHotkey.StreamDeck.PluginSettings.Instance.RotationIncrement;
        var ticks = payload.Ticks;

        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var delta = ticks * increment;
            var newValue = device.AdjustWheelRotation(delta);

            Connection.SetTitleAsync($"ROT\n{newValue}°");
            // Scale 90-2700 to 0-100 for indicator
            var indicatorValue = (newValue - 90) * 100 / (2700 - 90);
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{newValue}°" },
                { "indicator", indicatorValue.ToString() }
            });
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("ROT\nError");
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Rotation dial error: {ex.Message}");
        }
    }

    public override void DialDown(DialPayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var (_, currentValue) = device.GetWheelRotation();
            Connection.SetTitleAsync($"ROT\n{currentValue}°");
        }
        catch
        {
            Connection.SetTitleAsync("ROT\nError");
        }
    }

    public override void DialUp(DialPayload payload) { }
    public override void TouchPress(TouchpadPressPayload payload) { }
    public override void OnTick() { }
    public override void Dispose() { }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        Tools.AutoPopulateSettings(settings, payload.Settings);
        SaveSettings();
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

    private void SaveSettings()
    {
        Connection.SetSettingsAsync(JObject.FromObject(settings));
    }
}
