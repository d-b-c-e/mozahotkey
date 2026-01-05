using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.steeringwheelinertia")]
public class SteeringWheelInertiaAction : KeyAndEncoderBase
{
    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "direction")]
        public string Direction { get; set; } = "increase";

        [JsonProperty(PropertyName = "incrementValue")]
        public int IncrementValue { get; set; } = 50;
    }

    private PluginSettings settings;

    public SteeringWheelInertiaAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        if (payload.Settings == null || payload.Settings.Count == 0)
        {
            settings = PluginSettings.CreateDefaultSettings();
            Connection.SetSettingsAsync(JObject.FromObject(settings));
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
                var currentValue = MozaDeviceManager.Instance.Device.GetSteeringWheelInertia();
                await Connection.SetTitleAsync($"{currentValue}g");
                // Scale 100-1550 to 0-100 for indicator
                var indicatorValue = (currentValue - 100) * 100 / (1550 - 100);
                await Connection.SetFeedbackAsync(new Dictionary<string, string>
                {
                    { "value", $"{currentValue}g" },
                    { "indicator", indicatorValue.ToString() }
                });
                _initialized = true;
            }
            else
            {
                await Connection.SetTitleAsync("N/C");
            }
        }
        catch { await Connection.SetTitleAsync("N/C"); }
    }

    public override void KeyPressed(KeyPayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var delta = settings.Direction == "decrease" ? -settings.IncrementValue : settings.IncrementValue;
            var newValue = device.AdjustSteeringWheelInertia(delta);
            Connection.SetTitleAsync($"{newValue}g");
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Steering Wheel Inertia error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void DialRotate(DialRotatePayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var delta = payload.Ticks * settings.IncrementValue;
            var newValue = device.AdjustSteeringWheelInertia(delta);
            Connection.SetTitleAsync($"{newValue}g");
            // Scale 100-1550 to 0-100 for indicator
            var indicatorValue = (newValue - 100) * 100 / (1550 - 100);
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{newValue}g" },
                { "indicator", indicatorValue.ToString() }
            });
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Steering Wheel Inertia dial error: {ex.Message}");
        }
    }

    public override void DialDown(DialPayload payload)
    {
        try
        {
            var currentValue = MozaDeviceManager.Instance.Device.GetSteeringWheelInertia();
            Connection.SetTitleAsync($"{currentValue}g");
            var indicatorValue = (currentValue - 100) * 100 / (1550 - 100);
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{currentValue}g" },
                { "indicator", indicatorValue.ToString() }
            });
        }
        catch { }
    }

    private bool _initialized = false;

    public override void DialUp(DialPayload payload) { }
    public override void TouchPress(TouchpadPressPayload payload) { }

    public override void OnTick()
    {
        if (!_initialized)
        {
            InitializeDisplay();
        }
    }

    public override void Dispose() { }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        Tools.AutoPopulateSettings(settings, payload.Settings);
        Connection.SetSettingsAsync(JObject.FromObject(settings));
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
