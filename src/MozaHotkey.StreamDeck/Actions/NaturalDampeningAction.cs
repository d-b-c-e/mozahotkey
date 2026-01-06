using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.naturaldampening")]
public class NaturalDampeningAction : KeyAndEncoderBase
{
    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "direction")]
        public string Direction { get; set; } = "increase";

        [JsonProperty(PropertyName = "incrementValue")]
        public int IncrementValue { get; set; } = 5;
    }

    private PluginSettings settings;

    public NaturalDampeningAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
            UpdateDirectionIcon();
            if (MozaDeviceManager.Instance.TryInitialize())
            {
                var currentValue = MozaDeviceManager.Instance.Device.GetDamping();
                await Connection.SetTitleAsync($"{currentValue}%");
                await Connection.SetFeedbackAsync(new Dictionary<string, string>
                {
                    { "value", $"{currentValue}%" },
                    { "indicator", currentValue.ToString() }
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

    private void UpdateDirectionIcon()
    {
        var iconSuffix = settings.Direction == "increase" ? "Up" : "Down";
        Connection.SetImageAsync($"Images/dampingIcon{iconSuffix}.png");
    }

    public override void KeyPressed(KeyPayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var delta = settings.Direction == "decrease" ? -settings.IncrementValue : settings.IncrementValue;
            var newValue = device.AdjustDamping(delta);
            Connection.SetTitleAsync($"{newValue}%");
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Natural Dampening error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void DialRotate(DialRotatePayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var delta = payload.Ticks * settings.IncrementValue;
            var newValue = device.AdjustDamping(delta);
            Connection.SetTitleAsync($"{newValue}%");
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{newValue}%" },
                { "indicator", newValue.ToString() }
            });
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Natural Dampening dial error: {ex.Message}");
        }
    }

    public override void DialDown(DialPayload payload)
    {
        try
        {
            var currentValue = MozaDeviceManager.Instance.Device.GetDamping();
            Connection.SetTitleAsync($"{currentValue}%");
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
        UpdateDirectionIcon();
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
