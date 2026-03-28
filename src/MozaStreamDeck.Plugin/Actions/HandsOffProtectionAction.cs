using BarRaider.SdTools;

namespace MozaStreamDeck.Plugin.Actions;

[PluginActionId("com.dbce.moza-streamdeck.handsoffprotection")]
public class HandsOffProtectionAction : KeypadBase
{
    private static readonly string[] ModeLabels = ["OFF", "M1", "M2"];

    private bool _initialized = false;

    public HandsOffProtectionAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        InitializeDisplay();
        MozaDeviceManager.DeviceStateChanged += InitializeDisplay;
    }

    private async void InitializeDisplay()
    {
        try
        {
            if (MozaDeviceManager.Instance.IsReady)
            {
                var mode = MozaDeviceManager.Instance.Device.GetHandsOffProtection();
                await Connection.SetTitleAsync(ModeLabels[Math.Clamp(mode, 0, 2)]);
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
            if (!MozaDeviceManager.Instance.EnsureInitialized())
            {
                Connection.ShowAlert();
                return;
            }
            var device = MozaDeviceManager.Instance.Device;
            var newMode = device.ToggleHandsOffProtection();
            Connection.SetTitleAsync(ModeLabels[Math.Clamp(newMode, 0, 2)]);
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Hands-Off Protection error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void OnTick()
    {
        if (!_initialized && MozaDeviceManager.Instance.IsReady)
        {
            InitializeDisplay();
        }
    }

    public override void Dispose() => MozaDeviceManager.DeviceStateChanged -= InitializeDisplay;
    public override void ReceivedSettings(ReceivedSettingsPayload payload) { }
    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
