using BarRaider.SdTools;

namespace MozaStreamDeck.Plugin.Actions;

[PluginActionId("com.dbce.moza-streamdeck.clutchreverse")]
public class ClutchReverseAction : KeypadBase
{
    private bool _initialized = false;

    public ClutchReverseAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        InitializeDisplay();
        MozaDeviceManager.DeviceStateChanged += InitializeDisplay;
    }

    private async void InitializeDisplay()
    {
        try
        {
            if (MozaDeviceManager.Instance.TryInitialize())
            {
                var isReversed = MozaDeviceManager.Instance.Device.GetClutchReverse();
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
            var device = MozaDeviceManager.Instance.Device;
            var newState = device.ToggleClutchReverse();
            Connection.SetTitleAsync(newState ? "REV" : "NRM");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Clutch Reverse error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }

    public override void OnTick()
    {
        if (!_initialized)
        {
            InitializeDisplay();
        }
    }

    public override void Dispose() => MozaDeviceManager.DeviceStateChanged -= InitializeDisplay;
    public override void ReceivedSettings(ReceivedSettingsPayload payload) { }
    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
