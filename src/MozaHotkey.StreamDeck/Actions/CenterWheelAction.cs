using BarRaider.SdTools;

namespace MozaHotkey.StreamDeck.Actions;

[PluginActionId("com.mozahotkey.streamdeck.centerwheel")]
public class CenterWheelAction : KeypadBase
{
    public CenterWheelAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        Connection.SetTitleAsync("CENTER\nWheel");
    }

    public override void KeyPressed(KeyPayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            device.CenterWheel();
            Connection.SetTitleAsync("CENTER\n✓");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("CENTER\nError");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"CenterWheel error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }
    public override void OnTick() { }
    public override void Dispose() { }
    public override void ReceivedSettings(ReceivedSettingsPayload payload) { }
    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
