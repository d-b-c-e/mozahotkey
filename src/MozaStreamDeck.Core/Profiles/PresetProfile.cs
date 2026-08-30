using System.IO.Compression;
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
    /// Loads a preset from a Pit House motor preset file. Handles both the
    /// .mzpreset container Pit House writes now and the bare .json files
    /// older versions wrote.
    /// </summary>
    public static PresetProfile? LoadFromFile(string path)
    {
        try
        {
            using var doc = JsonDocument.Parse(ReadPresetJson(path));
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

    /// <summary>
    /// Pulls the preset JSON out of its container. A .mzpreset is a zip archive
    /// holding preset.json (plus metadata.json, which we don't need); anything
    /// else is read as bare JSON.
    /// </summary>
    private static string ReadPresetJson(string path)
    {
        using var stream = File.OpenRead(path);

        // "PK" is the zip magic number. Sniff the bytes rather than trusting the
        // extension, so a renamed or legacy-format file still loads.
        var isZip = stream.ReadByte() == 'P' && stream.ReadByte() == 'K';
        stream.Position = 0;

        if (!isZip)
        {
            using var plain = new StreamReader(stream);
            return plain.ReadToEnd();
        }

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var entry = archive.GetEntry("preset.json")
            ?? throw new InvalidDataException($"No preset.json inside {path}");
        using var packed = new StreamReader(entry.Open());
        return packed.ReadToEnd();
    }
}
