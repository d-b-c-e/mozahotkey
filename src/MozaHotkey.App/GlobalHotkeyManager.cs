using System.Runtime.InteropServices;
using MozaHotkey.Core.Settings;

namespace MozaHotkey.App;

/// <summary>
/// Manages global hotkey registration and callbacks using Windows API.
/// </summary>
public class GlobalHotkeyManager : IDisposable
{
    private const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly IntPtr _windowHandle;
    private readonly Dictionary<int, HotkeyBinding> _registeredHotkeys = new();
    private int _nextHotkeyId = 1;

    public event Action<HotkeyBinding>? HotkeyTriggered;

    public GlobalHotkeyManager(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
    }

    /// <summary>
    /// Registers a hotkey binding. Returns true if successful.
    /// </summary>
    public bool Register(HotkeyBinding binding)
    {
        if (!binding.HasHotkey) return false;

        var id = _nextHotkeyId++;
        var success = RegisterHotKey(_windowHandle, id, binding.Modifiers, (uint)binding.Key);

        if (success)
        {
            _registeredHotkeys[id] = binding;
        }

        return success;
    }

    /// <summary>
    /// Unregisters all hotkeys.
    /// </summary>
    public void UnregisterAll()
    {
        foreach (var id in _registeredHotkeys.Keys)
        {
            UnregisterHotKey(_windowHandle, id);
        }
        _registeredHotkeys.Clear();
    }

    /// <summary>
    /// Processes a Windows message. Returns true if it was a hotkey message.
    /// </summary>
    public bool ProcessMessage(ref Message m)
    {
        if (m.Msg != WM_HOTKEY) return false;

        var id = m.WParam.ToInt32();
        if (_registeredHotkeys.TryGetValue(id, out var binding))
        {
            HotkeyTriggered?.Invoke(binding);
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        UnregisterAll();
    }
}
