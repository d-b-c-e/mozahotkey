using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaStreamDeck.Plugin.Actions;

[PluginActionId("com.dbce.moza-streamdeck.roadsensitivity")]
public class RoadSensitivityAction : KeyAndEncoderBase
{
    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "direction")]
        public string Direction { get; set; } = "increase";

        [JsonProperty(PropertyName = "incrementValue")]
        public int IncrementValue { get; set; } = 1;
    }

    private PluginSettings settings;
    private bool _initialized = false;
    private readonly DateTime _startupTime = DateTime.UtcNow;
    private static readonly TimeSpan StartupGracePeriod = TimeSpan.FromSeconds(30);

    public RoadSensitivityAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
        MozaDeviceManager.DeviceStateChanged += InitializeDisplay;
    }

    private bool IsInStartupGracePeriod => DateTime.UtcNow - _startupTime < StartupGracePeriod;

    private async void InitializeDisplay()
    {
        try
        {
            UpdateDirectionIcon();
            if (MozaDeviceManager.Instance.TryInitialize())
            {
                var currentValue = MozaDeviceManager.Instance.Device.GetRoadSensitivity();

                // During startup grace period, treat 0 as "not connected yet" since
                // Moza Pit House may not be fully loaded.
                if (currentValue == 0 && IsInStartupGracePeriod)
                {
                    await Connection.SetTitleAsync("N/C");
                    await Connection.SetFeedbackAsync(new Dictionary<string, string>
                    {
                        { "value", "N/C" },
                        { "indicator", "0" }
                    });
                    return;
                }

                await Connection.SetTitleAsync($"{currentValue}");
                await Connection.SetFeedbackAsync(new Dictionary<string, string>
                {
                    { "value", $"{currentValue}/10" },
                    { "indicator", (currentValue * 10).ToString() }
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
        Connection.SetImageAsync($"Images/roadIcon{iconSuffix}.png");
    }

    public override void KeyPressed(KeyPayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var delta = settings.Direction == "decrease" ? -settings.IncrementValue : settings.IncrementValue;
            var newValue = device.AdjustRoadSensitivity(delta);
            Connection.SetTitleAsync($"{newValue}");
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Road Sensitivity error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void DialRotate(DialRotatePayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var delta = payload.Ticks * settings.IncrementValue;
            var newValue = device.AdjustRoadSensitivity(delta);
            Connection.SetTitleAsync($"{newValue}");
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{newValue}/10" },
                { "indicator", (newValue * 10).ToString() }
            });
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Road Sensitivity dial error: {ex.Message}");
        }
    }

    public override void DialDown(DialPayload payload)
    {
        try
        {
            var currentValue = MozaDeviceManager.Instance.Device.GetRoadSensitivity();
            Connection.SetTitleAsync($"{currentValue}");
        }
        catch { }
    }

    public override void DialUp(DialPayload payload) { }
    public override void TouchPress(TouchpadPressPayload payload) { }

    public override void OnTick()
    {
        if (!_initialized)
        {
            InitializeDisplay();
        }
    }

    public override void Dispose() => MozaDeviceManager.DeviceStateChanged -= InitializeDisplay;

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        Tools.AutoPopulateSettings(settings, payload.Settings);
        Connection.SetSettingsAsync(JObject.FromObject(settings));
        UpdateDirectionIcon();
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
