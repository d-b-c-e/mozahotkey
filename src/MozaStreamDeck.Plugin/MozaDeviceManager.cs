using BarRaider.SdTools;
using MozaStreamDeck.Core;

namespace MozaStreamDeck.Plugin;

/// <summary>
/// Singleton manager for MozaDevice lifecycle within the Stream Deck plugin.
/// Ensures only one connection to the Moza SDK is maintained.
/// SDK initialization is deferred until user interaction to avoid launching Pit House at boot.
/// </summary>
public sealed class MozaDeviceManager : IDisposable
{
    private static readonly Lazy<MozaDeviceManager> _instance = new(() => new MozaDeviceManager());
    public static MozaDeviceManager Instance => _instance.Value;

    private readonly MozaDevice _device;
    private bool _disposed;
    private bool _autoInitAttempted;
    private static readonly DateTime _startupTime = DateTime.UtcNow;

    private MozaDeviceManager()
    {
        _device = new MozaDevice();
    }

    /// <summary>
    /// Gets the MozaDevice instance. Throws if not initialized — callers must
    /// check IsReady or call TryInitialize() first (typically in KeyPressed/DialRotate).
    /// </summary>
    public MozaDevice Device
    {
        get
        {
            if (!_device.IsInitialized)
                throw new InvalidOperationException("Moza SDK not initialized. Call TryInitialize() first.");
            return _device;
        }
    }

    /// <summary>
    /// Gets whether the device is initialized and ready.
    /// </summary>
    public bool IsReady => _device.IsInitialized;

    /// <summary>
    /// Raised when device state changes externally (e.g., a preset was applied, or SDK first initialized).
    /// Subscribers should re-read their values from the device and refresh their displays.
    /// </summary>
    public static event Action? DeviceStateChanged;

    /// <summary>
    /// When set, contains the rotation value that was just written to the device.
    /// RotationAction should use this instead of reading from SDK (which may be stale).
    /// Expires after 5 seconds to prevent stale overrides.
    /// </summary>
    public static int? PendingRotationOverride { get; set; }
    public static DateTime PendingRotationOverrideTime { get; set; }

    /// <summary>
    /// Gets the rotation override if it's still fresh (within 5 seconds).
    /// </summary>
    public static int? GetRotationOverride()
    {
        if (PendingRotationOverride.HasValue &&
            (DateTime.UtcNow - PendingRotationOverrideTime).TotalSeconds < 5)
        {
            return PendingRotationOverride.Value;
        }
        PendingRotationOverride = null;
        return null;
    }

    /// <summary>
    /// Sets a rotation override with the current timestamp.
    /// </summary>
    public static void SetRotationOverride(int value)
    {
        PendingRotationOverride = value;
        PendingRotationOverrideTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Clears the rotation override (e.g., when the user manually changes rotation via dial).
    /// </summary>
    public static void ClearRotationOverride()
    {
        PendingRotationOverride = null;
    }

    /// <summary>
    /// Notifies all subscribers that device state has changed and displays should refresh.
    /// </summary>
    public static void NotifyStateChanged() => DeviceStateChanged?.Invoke();

    /// <summary>
    /// Attempts to initialize the device. Only call from user-interaction handlers
    /// (KeyPressed, DialRotate) to avoid launching Pit House at boot.
    /// </summary>
    public bool TryInitialize()
    {
        if (_device.IsInitialized) return true;
        return _device.Initialize();
    }

    /// <summary>
    /// Attempts one-time auto-initialization after startup grace period (10 seconds).
    /// Called from OnTick to populate displays without requiring user interaction.
    /// Only attempts once — if it fails (Pit House not running), waits for user interaction.
    /// </summary>
    public bool TryAutoInitialize()
    {
        if (_device.IsInitialized) return true;
        if (_autoInitAttempted) return false;
        if ((DateTime.UtcNow - _startupTime).TotalSeconds < 10) return false;

        _autoInitAttempted = true;
        Logger.Instance.LogMessage(TracingLevel.INFO, "AutoInit: attempting one-time SDK initialization...");
        if (!_device.Initialize())
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "AutoInit: failed (Pit House may not be running)");
            return false;
        }

        Logger.Instance.LogMessage(TracingLevel.INFO, "AutoInit: SDK initialized, notifying all actions");
        // Use delayed notifications like ForceRefresh — SDK needs time to populate
        Task.Run(async () =>
        {
            for (int i = 1; i <= 5; i++)
            {
                await Task.Delay(1000);
                Logger.Instance.LogMessage(TracingLevel.INFO, $"AutoInit: delayed notify attempt {i}/5");
                NotifyStateChanged();
            }
        });
        return true;
    }

    /// <summary>
    /// Initializes the SDK if needed and notifies all actions to refresh their displays.
    /// Returns false if initialization failed. Use in KeyPressed/DialRotate handlers.
    /// </summary>
    public bool EnsureInitialized()
    {
        if (_device.IsInitialized) return true;
        if (!_device.Initialize()) return false;
        NotifyStateChanged();
        return true;
    }

    /// <summary>
    /// Tears down the SDK connection, re-initializes it, and notifies all actions
    /// to refresh their displays. Use when values seem stale or out of sync.
    /// </summary>
    public bool ForceRefresh()
    {
        try
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "ForceRefresh: reinitializing device...");
            _device.Reinitialize();
            Logger.Instance.LogMessage(TracingLevel.INFO, "ForceRefresh: reinitialize complete, scheduling delayed notifications...");

            // The SDK takes several seconds to re-establish communication with Pit House
            // after reinitialize. Fire notifications at intervals to catch actions as
            // the SDK becomes ready (observed ~3.5s in testing).
            Task.Run(async () =>
            {
                for (int i = 1; i <= 5; i++)
                {
                    await Task.Delay(1000);
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ForceRefresh: delayed notify attempt {i}/5");
                    NotifyStateChanged();
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"ForceRefresh failed: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _device.Dispose();
        _disposed = true;
    }
}
