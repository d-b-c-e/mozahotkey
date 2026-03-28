using BarRaider.SdTools;

namespace MozaStreamDeck.Plugin.Actions;

[PluginActionId("com.dbce.moza-streamdeck.refresh")]
public class RefreshAction : KeypadBase
{
    public RefreshAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        Connection.SetTitleAsync("REFRESH");
    }

    public override void KeyPressed(KeyPayload payload)
    {
        try
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Refresh: reinitializing SDK connection");
            if (MozaDeviceManager.Instance.ForceRefresh())
            {
                Connection.SetTitleAsync("REFRESH");
                Connection.ShowOk();
                Logger.Instance.LogMessage(TracingLevel.INFO, "Refresh: SDK reinitialized, all displays notified");
            }
            else
            {
                Connection.SetTitleAsync("N/C");
                Connection.ShowAlert();
                Logger.Instance.LogMessage(TracingLevel.WARN, "Refresh: SDK reinitialization failed");
            }
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Refresh error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }
    public override void OnTick() { }
    public override void Dispose() { }
    public override void ReceivedSettings(ReceivedSettingsPayload payload) { }
    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
