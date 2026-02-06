namespace MozaHotkey.Core.Profiles;

/// <summary>
/// Manages discovery and loading of Moza Pit House motor presets.
/// </summary>
public static class PresetManager
{
    private static readonly string[] CandidatePaths =
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MOZA Pit House"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive", "Documents", "MOZA Pit House"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "MOZA Pit House"),
    };

    /// <summary>
    /// Finds the Moza Pit House user data directory.
    /// </summary>
    public static string? FindPitHouseDirectory()
    {
        foreach (var path in CandidatePaths)
        {
            if (Directory.Exists(Path.Combine(path, "Presets", "Motor")))
                return path;
        }
        return null;
    }

    /// <summary>
    /// Gets all motor presets from the Pit House presets directory.
    /// </summary>
    public static List<PresetProfile> GetMotorPresets()
    {
        var presets = new List<PresetProfile>();
        var pitHouseDir = FindPitHouseDirectory();
        if (pitHouseDir == null) return presets;

        var motorDir = Path.Combine(pitHouseDir, "Presets", "Motor");
        if (!Directory.Exists(motorDir)) return presets;

        foreach (var file in Directory.GetFiles(motorDir, "*.json"))
        {
            var preset = PresetProfile.LoadFromFile(file);
            if (preset != null && !string.IsNullOrWhiteSpace(preset.Name))
            {
                presets.Add(preset);
            }
        }

        presets.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        return presets;
    }
}
