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
                Logger.Instance.LogMessage(TracingLevel.INFO, "Rotation: InitializeDisplay called, device is ready");

                // If a preset was just applied, use its rotation value instead of
                // reading from the SDK (which returns a stale value for ~1 second).
                int currentValue;
                var rotationOverride = MozaDeviceManager.GetRotationOverride();
                if (rotationOverride.HasValue)
                {
                    currentValue = rotationOverride.Value;
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Rotation: using override value {currentValue} (skipping SDK read)");
                }
                else
                {
                    var (hwLimit, gameLimit) = MozaDeviceManager.Instance.Device.GetWheelRotation();
                    currentValue = gameLimit;
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Rotation: GetWheelRotation returned hw:{hwLimit}/game:{gameLimit}");
                }

                // During startup grace period, treat invalid values (< 90) as "not connected yet"
                // since Moza Pit House may not be fully loaded.
                // Skip this filter if already initialized (refresh scenario) — just wait for
                // the next retry to get the real value.
                if (currentValue < 90)
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Rotation: value {currentValue} < 90, skipping display update (waiting for SDK)");
                    if (!_initialized)
                    {
                        await Connection.SetTitleAsync("N/C");
                        await Connection.SetFeedbackAsync(new Dictionary<string, string>
                        {
                            { "value", "N/C" },
                            { "indicator", "0" }
                        });
                    }
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
                Logger.Instance.LogMessage(TracingLevel.INFO, $"Rotation: display updated to {currentValue}°");
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, "Rotation: InitializeDisplay called but device not ready");
                await Connection.SetTitleAsync("N/C");
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Rotation: InitializeDisplay failed: {ex.Message}");
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
            MozaDeviceManager.ClearRotationOverride();
            if (!MozaDeviceManager.Instance.EnsureInitialized())
            {
                Connection.ShowAlert();
                return;
            }
            var device = MozaDeviceManager.Instance.Device;
            var (hwBefore, gameBefore) = device.GetWheelRotation();
            var delta = settings.Direction == "decrease" ? -settings.IncrementValue : settings.IncrementValue;
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Rotation: KeyPressed delta={delta}, before=hw:{hwBefore}/game:{gameBefore}");
            var newValue = device.AdjustWheelRotation(delta);
            var (hwAfter, gameAfter) = device.GetWheelRotation();
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Rotation: KeyPressed set to {newValue}, readback=hw:{hwAfter}/game:{gameAfter}");
            if (gameAfter != newValue)
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Rotation: MISMATCH — set {newValue} but readback {gameAfter}");
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
            MozaDeviceManager.ClearRotationOverride();
            if (!MozaDeviceManager.Instance.EnsureInitialized())
            {
                Connection.ShowAlert();
                return;
            }
            var device = MozaDeviceManager.Instance.Device;
            var (hwBefore, gameBefore) = device.GetWheelRotation();
            var delta = payload.Ticks * settings.IncrementValue;
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Rotation: DialRotate ticks={payload.Ticks} delta={delta}, before=hw:{hwBefore}/game:{gameBefore}");
            var newValue = device.AdjustWheelRotation(delta);
            var (hwAfter, gameAfter) = device.GetWheelRotation();
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Rotation: DialRotate set to {newValue}, readback=hw:{hwAfter}/game:{gameAfter}");
            if (gameAfter != newValue)
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Rotation: MISMATCH — set {newValue} but readback {gameAfter}");

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
        if (!_initialized)
        {
            // Try auto-init once after startup (populates all displays without user interaction)
            MozaDeviceManager.Instance.TryAutoInitialize();

            if (MozaDeviceManager.Instance.IsReady)
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
