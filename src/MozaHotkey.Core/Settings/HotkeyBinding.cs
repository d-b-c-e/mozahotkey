using System.Text.Json.Serialization;

namespace MozaHotkey.Core.Settings;

/// <summary>
/// Represents a hotkey binding to an action.
/// </summary>
public class HotkeyBinding
{
    public string ActionId { get; set; } = string.Empty;
    public Keys Key { get; set; } = Keys.None;
    public bool Ctrl { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }

    [JsonIgnore]
    public bool HasHotkey => Key != Keys.None;

    [JsonIgnore]
    public string DisplayString
    {
        get
        {
            if (!HasHotkey) return "(Not Set)";

            var parts = new List<string>();
            if (Ctrl) parts.Add("Ctrl");
            if (Alt) parts.Add("Alt");
            if (Shift) parts.Add("Shift");
            parts.Add(Key.ToString());
            return string.Join(" + ", parts);
        }
    }

    /// <summary>
    /// Gets the modifier flags for Windows API RegisterHotKey.
    /// </summary>
    [JsonIgnore]
    public uint Modifiers
    {
        get
        {
            uint mods = 0;
            if (Alt) mods |= 0x0001;    // MOD_ALT
            if (Ctrl) mods |= 0x0002;   // MOD_CONTROL
            if (Shift) mods |= 0x0004;  // MOD_SHIFT
            return mods;
        }
    }
}

/// <summary>
/// Windows Forms Keys enumeration subset for JSON serialization.
/// </summary>
public enum Keys
{
    None = 0,
    A = 65, B = 66, C = 67, D = 68, E = 69, F = 70, G = 71, H = 72,
    I = 73, J = 74, K = 75, L = 76, M = 77, N = 78, O = 79, P = 80,
    Q = 81, R = 82, S = 83, T = 84, U = 85, V = 86, W = 87, X = 88,
    Y = 89, Z = 90,
    D0 = 48, D1 = 49, D2 = 50, D3 = 51, D4 = 52,
    D5 = 53, D6 = 54, D7 = 55, D8 = 56, D9 = 57,
    F1 = 112, F2 = 113, F3 = 114, F4 = 115, F5 = 116, F6 = 117,
    F7 = 118, F8 = 119, F9 = 120, F10 = 121, F11 = 122, F12 = 123,
    NumPad0 = 96, NumPad1 = 97, NumPad2 = 98, NumPad3 = 99, NumPad4 = 100,
    NumPad5 = 101, NumPad6 = 102, NumPad7 = 103, NumPad8 = 104, NumPad9 = 105,
    Add = 107, Subtract = 109, Multiply = 106, Divide = 111,
    Up = 38, Down = 40, Left = 37, Right = 39,
    Home = 36, End = 35, PageUp = 33, PageDown = 34,
    Insert = 45, Delete = 46,
    OemPlus = 187, OemMinus = 189, OemPeriod = 190, OemComma = 188
}
