using System.Diagnostics;
using BarRaider.SdTools;
using MozaStreamDeck.Core;

namespace MozaStreamDeck.Plugin;

/// <summary>
/// Singleton manager for MozaDevice lifecycle within the Stream Deck plugin.
/// Ensures only one connection to the Moza SDK is maintained.
/// SDK initialization is deferred until Pit House is detected running to avoid
/// the SDK's background process re-launching Pit House when the user closes it.
/// </summary>
public sealed class MozaDeviceManager : IDisposable
{
    private static readonly Lazy<MozaDeviceManager> _instance = new(() => new MozaDeviceManager());
    public static MozaDeviceManager Instance => _instance.Value;

    private readonly MozaDevice _device;
    private bool _disposed;
    private bool _autoInitAttempted;
    private static readonly DateTime _startupTime = DateTime.UtcNow;

    // Cached Pit House process check (avoid checking on every tick).
    // Short TTL so IsReady-driven teardown can't fall behind the SDK's relaunch heartbeat.
    private DateTime _lastPitHouseCheck = DateTime.MinValue;
    private bool _lastPitHouseRunning;
    private static readonly TimeSpan PitHouseCheckInterval = TimeSpan.FromSeconds(1);

    // Kernel-signal-based Pit House exit watcher. Fires teardown instantly on PH death,
    // beating the SDK's internal respawn heartbeat. Also kills any PH the SDK spawns
    // during the brief race window before Teardown() finishes.
    private CancellationTokenSource? _monitorCts;
    private Process? _monitoredPitHouse;

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
    /// Gets whether the device is initialized and Pit House is still running.
    /// If Pit House was closed while the SDK was active, tears down the SDK
    /// to prevent the SDK's background process from re-launching Pit House.
    /// </summary>
    public bool IsReady
    {
        get
        {
            if (!_device.IsInitialized) return false;
            if (!IsPitHouseRunning())
            {
                TeardownSdk();
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Checks if the Moza Pit House application is currently running.
    /// Result is cached for 5 seconds to avoid expensive process enumeration on every tick.
    /// </summary>
    public bool IsPitHouseRunning()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastPitHouseCheck) < PitHouseCheckInterval)
            return _lastPitHouseRunning;

        _lastPitHouseCheck = now;
        try
        {
            var processes = Process.GetProcessesByName("MOZA Pit House");
            _lastPitHouseRunning = processes.Length > 0;
            foreach (var p in processes) p.Dispose();
        }
        catch
        {
            _lastPitHouseRunning = false;
        }
        return _lastPitHouseRunning;
    }

    /// <summary>
    /// Tears down the SDK connection and resets state so it can re-initialize
    /// when Pit House comes back. Notifies all actions to refresh (show N/C).
    /// </summary>
    private void TeardownSdk()
    {
        if (!_device.IsInitialized) return;
        Logger.Instance.LogMessage(TracingLevel.INFO, "Pit House closed — tearing down SDK to prevent re-launch");
        _device.Teardown();
        _autoInitAttempted = false; // Allow re-init when Pit House comes back
        _lastPitHouseCheck = DateTime.MinValue; // Invalidate cache so next IsReady is authoritative
        _lastPitHouseRunning = false;
        _monitoredPitHouse?.Dispose();
        _monitoredPitHouse = null;
        NotifyStateChanged(); // Actions will see IsReady=false and show N/C
    }

    /// <summary>
    /// Hooks a kernel-level wait on the Pit House process so we get an instant signal
    /// when the user kills it — faster than the SDK's internal heartbeat detection.
    /// On exit: invalidates the cache, calls TeardownSdk(), and sweeps any Pit House
    /// process the SDK spawned during the race window before Teardown() completed.
    /// </summary>
    private void StartPitHouseMonitor()
    {
        // Cancel any prior watcher from an earlier init cycle.
        _monitorCts?.Cancel();
        _monitorCts = new CancellationTokenSource();
        var ct = _monitorCts.Token;

        Process? ph;
        try
        {
            ph = Process.GetProcessesByName("MOZA Pit House").FirstOrDefault();
        }
        catch
        {
            ph = null;
        }

        if (ph == null)
        {
            Logger.Instance.LogMessage(TracingLevel.WARN,
                "PitHouseMonitor: no Pit House process found at SDK init time — teardown-on-exit disabled");
            return;
        }

        _monitoredPitHouse?.Dispose();
        _monitoredPitHouse = ph;
        var phId = ph.Id;
        Logger.Instance.LogMessage(TracingLevel.INFO,
            $"PitHouseMonitor: watching Pit House PID {phId} for exit");

        _ = Task.Run(async () =>
        {
            try
            {
                await ph.WaitForExitAsync(ct).ConfigureAwait(false);
                if (ct.IsCancellationRequested) return;

                Logger.Instance.LogMessage(TracingLevel.INFO,
                    $"PitHouseMonitor: Pit House (PID {phId}) exited — tearing down SDK immediately");

                // Tear down first. This stops the SDK from spawning further PH processes.
                TeardownSdk();

                // Belt-and-suspenders: if the SDK already spawned a replacement during the
                // race between PH dying and Teardown() completing, kill those replacements.
                // After Teardown() succeeds the SDK is unloaded and won't spawn more.
                for (int i = 0; i < 15 && !ct.IsCancellationRequested; i++)
                {
                    try
                    {
                        var spawned = Process.GetProcessesByName("MOZA Pit House");
                        foreach (var p in spawned)
                        {
                            try
                            {
                                Logger.Instance.LogMessage(TracingLevel.INFO,
                                    $"PitHouseMonitor: killing SDK-respawned Pit House PID {p.Id}");
                                p.Kill();
                            }
                            catch (Exception killEx)
                            {
                                Logger.Instance.LogMessage(TracingLevel.WARN,
                                    $"PitHouseMonitor: failed to kill PID {p.Id}: {killEx.Message}");
                            }
                            finally { p.Dispose(); }
                        }
                    }
                    catch { /* enumeration transient failures are fine */ }

                    try { await Task.Delay(200, ct).ConfigureAwait(false); }
                    catch (OperationCanceledException) { break; }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR,
                    $"PitHouseMonitor exception: {ex.Message}");
            }
        }, ct);
    }

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
    /// (KeyPressed, DialRotate). Will not initialize unless Pit House is running.
    /// </summary>
    public bool TryInitialize()
    {
        if (_device.IsInitialized) return true;
        if (!IsPitHouseRunning()) return false;
        if (!_device.Initialize()) return false;
        StartPitHouseMonitor();
        return true;
    }

    /// <summary>
    /// Attempts auto-initialization after startup grace period (10 seconds).
    /// Called from OnTick to populate displays without requiring user interaction.
    /// Will not initialize unless Pit House is running. Re-initializes automatically
    /// if Pit House is restarted after a previous teardown.
    /// </summary>
    public bool TryAutoInitialize()
    {
        if (_device.IsInitialized) return true; // IsReady handles teardown if PH closed
        if ((DateTime.UtcNow - _startupTime).TotalSeconds < 10) return false;

        if (!IsPitHouseRunning())
        {
            // Don't set _autoInitAttempted — we'll try again when Pit House starts
            return false;
        }

        if (_autoInitAttempted) return false;
        _autoInitAttempted = true;

        Logger.Instance.LogMessage(TracingLevel.INFO, "AutoInit: Pit House detected, attempting SDK initialization...");
        if (!_device.Initialize())
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "AutoInit: failed despite Pit House running");
            return false;
        }

        StartPitHouseMonitor();
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
    /// Returns false if Pit House is not running or initialization failed.
    /// Use in KeyPressed/DialRotate handlers.
    /// </summary>
    public bool EnsureInitialized()
    {
        if (_device.IsInitialized) return true;
        if (!IsPitHouseRunning()) return false;
        if (!_device.Initialize()) return false;
        StartPitHouseMonitor();
        NotifyStateChanged();
        return true;
    }

    /// <summary>
    /// Tears down the SDK connection, re-initializes it, and notifies all actions
    /// to refresh their displays. Use when values seem stale or out of sync.
    /// Returns false if Pit House is not running.
    /// </summary>
    public bool ForceRefresh()
    {
        try
        {
            if (!IsPitHouseRunning())
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, "ForceRefresh: Pit House not running, cannot refresh");
                return false;
            }

            Logger.Instance.LogMessage(TracingLevel.INFO, "ForceRefresh: reinitializing device...");
            _device.Reinitialize();
            StartPitHouseMonitor();
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
        _monitorCts?.Cancel();
        _monitoredPitHouse?.Dispose();
        _device.Dispose();
        _disposed = true;
    }
}
