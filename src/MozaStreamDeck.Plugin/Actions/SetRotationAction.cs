using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MozaStreamDeck.Plugin.Actions;

[PluginActionId("com.dbce.moza-streamdeck.setrotation")]
public class SetRotationAction : KeypadBase
{
    private class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings() => new();

        [JsonProperty(PropertyName = "degrees")]
        public int Degrees { get; set; } = 540;
    }

    private PluginSettings settings;

    public SetRotationAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        settings = payload.Settings?.ToObject<PluginSettings>() ?? PluginSettings.CreateDefaultSettings();
        Connection.SetTitleAsync($"{settings.Degrees}°");
    }

    public override void KeyPressed(KeyPayload payload)
    {
        try
        {
            var device = MozaDeviceManager.Instance.Device;
            device.SetWheelRotation(settings.Degrees);
            Connection.SetTitleAsync($"{settings.Degrees}°");
            Connection.ShowOk();
        }
        catch (Exception ex)
        {
            Connection.SetTitleAsync("Error");
            Connection.ShowAlert();
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"SetRotation error: {ex.Message}");
        }
    }

    public override void KeyReleased(KeyPayload payload) { }
    public override void OnTick() { }
    public override void Dispose() { }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        Tools.AutoPopulateSettings(settings, payload.Settings);
        Connection.SetSettingsAsync(JObject.FromObject(settings));
        Connection.SetTitleAsync($"{settings.Degrees}°");
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
}
