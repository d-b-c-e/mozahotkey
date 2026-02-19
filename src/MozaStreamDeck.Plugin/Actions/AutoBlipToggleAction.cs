using BarRaider.SdTools;

namespace MozaStreamDeck.Plugin.Actions;

[PluginActionId("com.dbce.moza-streamdeck.autoblip")]
public class AutoBlipToggleAction : KeypadBase
{
    private bool _initialized = false;

    public AutoBlipToggleAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
                var isEnabled = MozaDeviceManager.Instance.Device.GetAutoBlipEnabled();
                await Connection.SetTitleAsync(isEnabled ? "ON" : "OFF");
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
            var newState = device.ToggleAutoBlip();
            Connection.SetTitleAsync(newState ? "ON" : "OFF");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Auto-Blip toggle error: {ex.Message}");
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
