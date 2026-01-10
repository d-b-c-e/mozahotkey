using BarRaider.SdTools;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.brakereverse")]
public class BrakeReverseAction : KeypadBase
{
    private bool _initialized = false;

    public BrakeReverseAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        InitializeDisplay();
    }

    private async void InitializeDisplay()
    {
        try
        {
            if (MozaDeviceManager.Instance.TryInitialize())
            {
                var isReversed = MozaDeviceManager.Instance.Device.GetBrakeReverse();
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
            var newState = device.ToggleBrakeReverse();
            Connection.SetTitleAsync(newState ? "REV" : "NRM");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Brake Reverse error: {ex.Message}");
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

    public override void Dispose() { }
    public override void ReceivedSettings(ReceivedSettingsPayload payload) { }
    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
