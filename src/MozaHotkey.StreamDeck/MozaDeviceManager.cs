using MozaHotkey.Core;

namespace MozaHotkey.StreamDeck;

/// <summary>
/// Singleton manager for MozaDevice lifecycle within the Stream Deck plugin.
/// Ensures only one connection to the Moza SDK is maintained.
/// </summary>
public sealed class MozaDeviceManager : IDisposable
{
    private static readonly Lazy<MozaDeviceManager> _instance = new(() => new MozaDeviceManager());
    public static MozaDeviceManager Instance => _instance.Value;

    private readonly MozaDevice _device;
    private bool _disposed;

    private MozaDeviceManager()
    {
        _device = new MozaDevice();
    }

    /// <summary>
    /// Gets the MozaDevice instance, initializing if necessary.
    /// </summary>
    public MozaDevice Device
    {
        get
        {
            if (!_device.IsInitialized)
            {
                _device.Initialize();
            }
            return _device;
        }
    }

    /// <summary>
    /// Gets whether the device is initialized and ready.
    /// </summary>
    public bool IsReady => _device.IsInitialized;

    /// <summary>
    /// Attempts to initialize the device.
    /// </summary>
    public bool TryInitialize()
    {
        if (_device.IsInitialized) return true;
        return _device.Initialize();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _device.Dispose();
        _disposed = true;
    }
}
