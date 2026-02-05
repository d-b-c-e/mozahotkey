using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.autoblipduration")]
public class AutoBlipDurationAction : KeyAndEncoderBase
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
    private bool _initialized = false;
    private readonly DateTime _startupTime = DateTime.UtcNow;
    private static readonly TimeSpan StartupGracePeriod = TimeSpan.FromSeconds(30);

    public AutoBlipDurationAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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

    private bool IsInStartupGracePeriod => DateTime.UtcNow - _startupTime < StartupGracePeriod;

    private async void InitializeDisplay()
    {
        try
        {
            UpdateDirectionIcon();
            if (MozaDeviceManager.Instance.TryInitialize())
            {
                var currentValue = MozaDeviceManager.Instance.Device.GetAutoBlipDuration();

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

                // Scale 0-500 to 0-100 for LCD indicator bar
                var indicatorValue = currentValue * 100 / 500;
                await Connection.SetTitleAsync($"{currentValue}ms");
                await Connection.SetFeedbackAsync(new Dictionary<string, string>
                {
                    { "value", $"{currentValue}ms" },
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
        Connection.SetImageAsync($"Images/blipIcon{iconSuffix}.png");
    }

    public override void KeyPressed(KeyPayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var delta = settings.Direction == "decrease" ? -settings.IncrementValue : settings.IncrementValue;
            var newValue = device.AdjustAutoBlipDuration(delta);
            Connection.SetTitleAsync($"{newValue}ms");
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Auto-Blip duration error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void DialRotate(DialRotatePayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            var delta = payload.Ticks * settings.IncrementValue;
            var newValue = device.AdjustAutoBlipDuration(delta);

            var indicatorValue = newValue * 100 / 500;
            Connection.SetTitleAsync($"{newValue}ms");
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{newValue}ms" },
                { "indicator", indicatorValue.ToString() }
            });
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Auto-Blip duration dial error: {ex.Message}");
        }
    }

    public override void DialDown(DialPayload payload)
    {
        try
        {
            var currentValue = MozaDeviceManager.Instance.Device.GetAutoBlipDuration();
            var indicatorValue = currentValue * 100 / 500;
            Connection.SetTitleAsync($"BLIP\n{currentValue}ms");
            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                { "value", $"{currentValue}ms" },
                { "indicator", indicatorValue.ToString() }
            });
        }
        catch
        {
            Connection.SetTitleAsync("Error");
        }
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

    public override void Dispose() { }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        Tools.AutoPopulateSettings(settings, payload.Settings);
        Connection.SetSettingsAsync(JObject.FromObject(settings));
        UpdateDirectionIcon();
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
