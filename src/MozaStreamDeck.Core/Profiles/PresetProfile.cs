using System.Text.Json;

namespace MozaStreamDeck.Core.Profiles;

/// <summary>
/// Represents a parsed Moza Pit House motor preset.
/// </summary>
public class PresetProfile
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string FilePath { get; set; } = "";
    public List<string> Devices { get; set; } = new();
    public Dictionary<string, object> DeviceParams { get; set; } = new();

    /// <summary>
    /// Loads a preset from a Pit House motor preset JSON file.
    /// </summary>
    public static PresetProfile? LoadFromFile(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var profile = new PresetProfile
            {
                FilePath = path,
                Id = root.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
                Name = root.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
            };

            if (root.TryGetProperty("devices", out var devices) && devices.ValueKind == JsonValueKind.Array)
            {
                foreach (var device in devices.EnumerateArray())
                {
                    var val = device.GetString();
                    if (val != null) profile.Devices.Add(val);
                }
            }

            if (root.TryGetProperty("deviceParams", out var deviceParams) && deviceParams.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in deviceParams.EnumerateObject())
                {
                    profile.DeviceParams[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.Number => prop.Value.TryGetInt32(out var i) ? i : prop.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.String => prop.Value.GetString() ?? "",
                        _ => prop.Value.ToString()
                    };
                }
            }

            return profile;
        }
        catch
        {
            return null;
        }
    }
}
