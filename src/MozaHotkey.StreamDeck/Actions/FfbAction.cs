using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaHotkey.StreamDeck.Actions;

/// <summary>
/// Stream Deck action for adjusting FFB strength.
/// Supports both buttons and dials.
/// </summary>
[PluginActionId("com.mozahotkey.streamdeck.ffb")]
public class FfbAction : KeyAndEncoderBase
{
    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "direction")]
        public string Direction { get; set; } = "increase";
    }

    private PluginSettings settings;

    public FfbAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
                var currentValue = MozaDeviceManager.Instance.Device.GetFfbStrength();
                await Connection.SetTitleAsync($"FFB\n{currentValue}%");
            }
            else
            {
                await Connection.SetTitleAsync("FFB\nN/C");
            }
        }
        catch
        {
            await Connection.SetTitleAsync("FFB\nN/C");
        }
    }

    public override void KeyPressed(KeyPayload payload)
    {
        var increment = MozaHotkey.StreamDeck.PluginSettings.Instance.FfbIncrement;

        try
        {
            var device = MozaDeviceManager.Instance.Device;
            int newValue;

            if (settings.Direction == "decrease")
            {
                newValue = device.AdjustFfbStrength(-increment);
            }
            else
            {
                newValue = device.AdjustFfbStrength(increment);
            }

            Connection.SetTitleAsync($"FFB\n{newValue}%");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("FFB\nError");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"FFB action error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void DialRotate(DialRotatePayload payload)
    {
        var increment = MozaHotkey.StreamDeck.PluginSettings.Instance.FfbIncrement;
        var ticks = payload.Ticks;

        try
        {
            var device = MozaDeviceManager.Instance.Device;
            // Each tick adjusts by the configured increment
            var delta = ticks * increment;
            var newValue = device.AdjustFfbStrength(delta);

            Connection.SetTitleAsync($"FFB\n{newValue}%");
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{newValue}%" },
                { "indicator", newValue.ToString() }
            });
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("FFB\nError");
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"FFB dial error: {ex.Message}");
        }
    }

    public override void DialDown(DialPayload payload)
    {
        // Show current value when dial is pressed
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var currentValue = device.GetFfbStrength();
            Connection.SetTitleAsync($"FFB\n{currentValue}%");
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{currentValue}%" },
                { "indicator", currentValue.ToString() }
            });
        }
        catch
        {
            Connection.SetTitleAsync("FFB\nError");
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
