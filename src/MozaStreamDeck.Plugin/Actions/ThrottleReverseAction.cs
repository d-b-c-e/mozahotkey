using BarRaider.SdTools;

namespace MozaStreamDeck.Plugin.Actions;

[PluginActionId("com.dbce.moza-streamdeck.throttlereverse")]
public class ThrottleReverseAction : KeypadBase
{
    private bool _initialized = false;

    public ThrottleReverseAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
                var isReversed = MozaDeviceManager.Instance.Device.GetThrottleReverse();
                await Connection.SetTitleAsync(isReversed ? "REV" : "NRM");
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
            var newState = device.ToggleThrottleReverse();
            Connection.SetTitleAsync(newState ? "REV" : "NRM");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Throttle Reverse error: {ex.Message}");
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
