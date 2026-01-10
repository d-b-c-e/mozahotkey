using BarRaider.SdTools;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.stopffb")]
public class StopFfbAction : KeypadBase
{
    public StopFfbAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        Connection.SetTitleAsync("STOP");
    }

    public override void KeyPressed(KeyPayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            device.StopForceFeedback();
            Connection.SetTitleAsync("STOP");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Stop FFB error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }
    public override void OnTick() { }
    public override void Dispose() { }
    public override void ReceivedSettings(ReceivedSettingsPayload payload) { }
    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
