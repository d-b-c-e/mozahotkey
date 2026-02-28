using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaStreamDeck.Plugin.Actions;

[PluginActionId("com.dbce.moza-streamdeck.rotation")]
public class RotationAction : KeyAndEncoderBase
{
    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "direction")]
        public string Direction { get; set; } = "increase";

        [JsonProperty(PropertyName = "incrementValue")]
        public int IncrementValue { get; set; } = 90;
    }

    private PluginSettings settings;
    private readonly DateTime _startupTime = DateTime.UtcNow;
    private static readonly TimeSpan StartupGracePeriod = TimeSpan.FromSeconds(30);

    public RotationAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
            if (MozaDeviceManager.Instance.IsReady)
            {
                var (_, currentValue) = MozaDeviceManager.Instance.Device.GetWheelRotation();

                // During startup grace period, treat invalid values (< 90) as "not connected yet"
                // since Moza Pit House may not be fully loaded.
                if (currentValue < 90 && IsInStartupGracePeriod)
                {
                    await Connection.SetTitleAsync("N/C");
                    await Connection.SetFeedbackAsync(new Dictionary<string, string>
                    {
                        { "value", "N/C" },
                        { "indicator", "0" }
                    });
                    return;
                }

                await Connection.SetTitleAsync($"{currentValue}°");
                var indicatorValue = (currentValue - 90) * 100 / (2700 - 90);
                await Connection.SetFeedbackAsync(new Dictionary<string, string>
                {
                    { "value", $"{currentValue}°" },
                    { "indicator", indicatorValue.ToString() }
                });
                _initialized = true;
            }
            else
            {
                await Connection.SetTitleAsync("N/C");
            }
        }
        catch
        {
            await Connection.SetTitleAsync("N/C");
        }
    }

    private void UpdateDirectionIcon()
    {
        var iconSuffix = settings.Direction == "increase" ? "Up" : "Down";
        Connection.SetImageAsync($"Images/rotationIcon{iconSuffix}.png");
    }

    public override void KeyPressed(KeyPayload payload)
    {
        try
        {
            if (!MozaDeviceManager.Instance.EnsureInitialized())
            {
                Connection.ShowAlert();
                return;
            }
            var device = MozaDeviceManager.Instance.Device;
            var delta = settings.Direction == "decrease" ? -settings.IncrementValue : settings.IncrementValue;
            var newValue = device.AdjustWheelRotation(delta);
            Connection.SetTitleAsync($"{newValue}°");
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Rotation action error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void DialRotate(DialRotatePayload payload)
    {
        try
        {
            if (!MozaDeviceManager.Instance.EnsureInitialized())
            {
                Connection.ShowAlert();
                return;
            }
            var device = MozaDeviceManager.Instance.Device;
            var delta = payload.Ticks * settings.IncrementValue;
            var newValue = device.AdjustWheelRotation(delta);

            Connection.SetTitleAsync($"{newValue}°");
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
            Connection.SetTitleAsync("Error");
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Rotation dial error: {ex.Message}");
        }
    }

    public override void DialDown(DialPayload payload)
    {
        if (!MozaDeviceManager.Instance.IsReady) return;
        try
        {
            var (_, currentValue) = MozaDeviceManager.Instance.Device.GetWheelRotation();
            Connection.SetTitleAsync($"{currentValue}°");
        }
        catch
        {
            Connection.SetTitleAsync("Error");
        }
    }

    private bool _initialized = false;

    public override void DialUp(DialPayload payload) { }
    public override void TouchPress(TouchpadPressPayload payload) { }

    public override void OnTick()
    {
        if (!_initialized && MozaDeviceManager.Instance.IsReady)
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
