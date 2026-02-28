using mozaAPI;
using static mozaAPI.mozaAPI;
using MozaStreamDeck.Core.Profiles;

namespace MozaStreamDeck.Core;

/// <summary>
/// Wrapper around the Moza SDK providing a clean C# interface for wheel base control.
/// </summary>
public class MozaDevice : IDisposable
{
    private bool _initialized;
    private bool _disposed;

    public bool IsInitialized => _initialized;

    /// <summary>
    /// Initializes the Moza SDK. Must be called before any other operations.
    /// </summary>
    public bool Initialize()
    {
        if (_initialized) return true;

        try
        {
            installMozaSDK();
            _initialized = true;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the current Force Feedback strength (0-100).
    /// </summary>
    public int GetFfbStrength()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorFfbStrength(ref error);
        ThrowIfError(error, "Failed to get FFB strength");
        return result;
    }

    /// <summary>
    /// Sets the Force Feedback strength (0-100).
    /// </summary>
    public void SetFfbStrength(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 0, 100);
        var error = setMotorFfbStrength(value);
        ThrowIfError(error, "Failed to set FFB strength");
    }

    /// <summary>
    /// Adjusts FFB strength by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustFfbStrength(int delta)
    {
        var current = GetFfbStrength();
        var newValue = Math.Clamp(current + delta, 0, 100);
        SetFfbStrength(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the current wheel rotation angle limits.
    /// Returns (hardwareLimit, gameLimit) where values are in degrees (90-2700).
    /// </summary>
    public (int HardwareLimit, int GameLimit) GetWheelRotation()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorLimitAngle(ref error);
        ThrowIfError(error, "Failed to get wheel rotation");

        if (result == null)
            throw new MozaException("Failed to get wheel rotation: null result");

        return (result.Item1, result.Item2);
    }

    /// <summary>
    /// Sets the wheel rotation angle (90-2700 degrees).
    /// Sets both hardware and game limit to the same value.
    /// </summary>
    public void SetWheelRotation(int degrees)
    {
        EnsureInitialized();
        degrees = Math.Clamp(degrees, 90, 2700);
        var error = setMotorLimitAngle(degrees, degrees);
        ThrowIfError(error, "Failed to set wheel rotation");
    }

    /// <summary>
    /// Sets the wheel rotation with separate hardware and game limits (90-2700 degrees each).
    /// </summary>
    public void SetWheelRotation(int hardwareLimit, int gameLimit)
    {
        EnsureInitialized();
        hardwareLimit = Math.Clamp(hardwareLimit, 90, 2700);
        gameLimit = Math.Clamp(gameLimit, 90, 2700);
        var error = setMotorLimitAngle(hardwareLimit, gameLimit);
        ThrowIfError(error, "Failed to set wheel rotation");
    }

    /// <summary>
    /// Adjusts wheel rotation by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustWheelRotation(int delta)
    {
        var (_, current) = GetWheelRotation();
        var newValue = Math.Clamp(current + delta, 90, 2700);
        SetWheelRotation(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the road sensitivity (0-10).
    /// </summary>
    public int GetRoadSensitivity()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorRoadSensitivity(ref error);
        ThrowIfError(error, "Failed to get road sensitivity");
        return result;
    }

    /// <summary>
    /// Sets the road sensitivity (0-10).
    /// </summary>
    public void SetRoadSensitivity(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 0, 10);
        var error = setMotorRoadSensitivity(value);
        ThrowIfError(error, "Failed to set road sensitivity");
    }

    /// <summary>
    /// Adjusts road sensitivity by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustRoadSensitivity(int delta)
    {
        var current = GetRoadSensitivity();
        var newValue = Math.Clamp(current + delta, 0, 10);
        SetRoadSensitivity(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the max torque limit (50-100).
    /// </summary>
    public int GetMaxTorque()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorPeakTorque(ref error);
        ThrowIfError(error, "Failed to get max torque");
        return result;
    }

    /// <summary>
    /// Sets the max torque limit (50-100).
    /// </summary>
    public void SetMaxTorque(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 50, 100);
        var error = setMotorPeakTorque(value);
        ThrowIfError(error, "Failed to set max torque");
    }

    /// <summary>
    /// Adjusts max torque by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustMaxTorque(int delta)
    {
        var current = GetMaxTorque();
        var newValue = Math.Clamp(current + delta, 50, 100);
        SetMaxTorque(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the damping strength (0-100).
    /// </summary>
    public int GetDamping()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorNaturalDamper(ref error);
        ThrowIfError(error, "Failed to get damping");
        return result;
    }

    /// <summary>
    /// Sets the damping strength (0-100).
    /// </summary>
    public void SetDamping(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 0, 100);
        var error = setMotorNaturalDamper(value);
        ThrowIfError(error, "Failed to set damping");
    }

    /// <summary>
    /// Adjusts damping by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustDamping(int delta)
    {
        var current = GetDamping();
        var newValue = Math.Clamp(current + delta, 0, 100);
        SetDamping(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the spring/center force strength (0-100).
    /// </summary>
    public int GetSpringStrength()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorSpringStrength(ref error);
        ThrowIfError(error, "Failed to get spring strength");
        return result;
    }

    /// <summary>
    /// Sets the spring/center force strength (0-100).
    /// </summary>
    public void SetSpringStrength(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 0, 100);
        var error = setMotorSpringStrength(value);
        ThrowIfError(error, "Failed to set spring strength");
    }

    /// <summary>
    /// Adjusts spring strength by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustSpringStrength(int delta)
    {
        var current = GetSpringStrength();
        var newValue = Math.Clamp(current + delta, 0, 100);
        SetSpringStrength(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the natural friction (0-100).
    /// </summary>
    public int GetNaturalFriction()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorNaturalFriction(ref error);
        ThrowIfError(error, "Failed to get natural friction");
        return result;
    }

    /// <summary>
    /// Sets the natural friction (0-100).
    /// </summary>
    public void SetNaturalFriction(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 0, 100);
        var error = setMotorNaturalFriction(value);
        ThrowIfError(error, "Failed to set natural friction");
    }

    /// <summary>
    /// Adjusts natural friction by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustNaturalFriction(int delta)
    {
        var current = GetNaturalFriction();
        var newValue = Math.Clamp(current + delta, 0, 100);
        SetNaturalFriction(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the natural inertia (100-500).
    /// </summary>
    public int GetNaturalInertia()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorNaturalInertia(ref error);
        ThrowIfError(error, "Failed to get natural inertia");
        return result;
    }

    /// <summary>
    /// Sets the natural inertia (100-500).
    /// </summary>
    public void SetNaturalInertia(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 100, 500);
        var error = setMotorNaturalInertia(value);
        ThrowIfError(error, "Failed to set natural inertia");
    }

    /// <summary>
    /// Adjusts natural inertia by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustNaturalInertia(int delta)
    {
        var current = GetNaturalInertia();
        var newValue = Math.Clamp(current + delta, 100, 500);
        SetNaturalInertia(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the steering wheel inertia ratio (100-1550).
    /// Simulates steering wheels with different weights.
    /// </summary>
    public int GetSteeringWheelInertia()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorNaturalInertiaRatio(ref error);
        ThrowIfError(error, "Failed to get steering wheel inertia");
        return result;
    }

    /// <summary>
    /// Sets the steering wheel inertia ratio (100-1550).
    /// </summary>
    public void SetSteeringWheelInertia(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 100, 1550);
        var error = setMotorNaturalInertiaRatio(value);
        ThrowIfError(error, "Failed to set steering wheel inertia");
    }

    /// <summary>
    /// Adjusts steering wheel inertia by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustSteeringWheelInertia(int delta)
    {
        var current = GetSteeringWheelInertia();
        var newValue = Math.Clamp(current + delta, 100, 1550);
        SetSteeringWheelInertia(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the maximum wheel speed limit (0-100).
    /// </summary>
    public int GetMaxWheelSpeed()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorLimitWheelSpeed(ref error);
        ThrowIfError(error, "Failed to get max wheel speed");
        return result;
    }

    /// <summary>
    /// Sets the maximum wheel speed limit (0-100).
    /// </summary>
    public void SetMaxWheelSpeed(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 0, 100);
        var error = setMotorLimitWheelSpeed(value);
        ThrowIfError(error, "Failed to set max wheel speed");
    }

    /// <summary>
    /// Adjusts max wheel speed by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustMaxWheelSpeed(int delta)
    {
        var current = GetMaxWheelSpeed();
        var newValue = Math.Clamp(current + delta, 0, 100);
        SetMaxWheelSpeed(newValue);
        return newValue;
    }

    /// <summary>
    /// Centers the wheel.
    /// </summary>
    public void CenterWheel()
    {
        EnsureInitialized();
        var error = mozaAPI.mozaAPI.CenterWheel();
        ThrowIfError(error, "Failed to center wheel");
    }

    /// <summary>
    /// Gets whether FFB direction is reversed.
    /// </summary>
    public bool GetFfbReverse()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorFfbReverse(ref error);
        ThrowIfError(error, "Failed to get FFB reverse");
        return result != 0;
    }

    /// <summary>
    /// Sets whether FFB direction is reversed.
    /// </summary>
    public void SetFfbReverse(bool reversed)
    {
        EnsureInitialized();
        var error = setMotorFfbReverse(reversed ? 1 : 0);
        ThrowIfError(error, "Failed to set FFB reverse");
    }

    /// <summary>
    /// Toggles FFB direction reverse and returns the new state.
    /// </summary>
    public bool ToggleFfbReverse()
    {
        var current = GetFfbReverse();
        SetFfbReverse(!current);
        return !current;
    }

    /// <summary>
    /// Gets the speed damping value (0-100).
    /// </summary>
    public int GetSpeedDamping()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorSpeedDamping(ref error);
        ThrowIfError(error, "Failed to get speed damping");
        return result;
    }

    /// <summary>
    /// Sets the speed damping value (0-100).
    /// </summary>
    public void SetSpeedDamping(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 0, 100);
        var error = setMotorSpeedDamping(value);
        ThrowIfError(error, "Failed to set speed damping");
    }

    /// <summary>
    /// Adjusts speed damping by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustSpeedDamping(int delta)
    {
        var current = GetSpeedDamping();
        var newValue = Math.Clamp(current + delta, 0, 100);
        SetSpeedDamping(newValue);
        return newValue;
    }

    /// <summary>
    /// Stops all force feedback immediately.
    /// </summary>
    public void StopForceFeedback()
    {
        EnsureInitialized();
        var error = stopForceFeedback();
        ThrowIfError(error, "Failed to stop force feedback");
    }

    /// <summary>
    /// Gets whether throttle pedal output is reversed.
    /// </summary>
    public bool GetThrottleReverse()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getPedalAccOutDir(ref error);
        ThrowIfError(error, "Failed to get throttle reverse");
        return result != 0;
    }

    /// <summary>
    /// Sets whether throttle pedal output is reversed.
    /// </summary>
    public void SetThrottleReverse(bool reversed)
    {
        EnsureInitialized();
        var error = setPedalAccOutDir(reversed ? 1 : 0);
        ThrowIfError(error, "Failed to set throttle reverse");
    }

    /// <summary>
    /// Toggles throttle pedal reverse and returns the new state.
    /// </summary>
    public bool ToggleThrottleReverse()
    {
        var current = GetThrottleReverse();
        SetThrottleReverse(!current);
        return !current;
    }

    /// <summary>
    /// Gets whether brake pedal output is reversed.
    /// </summary>
    public bool GetBrakeReverse()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getPedalBrakeOutDir(ref error);
        ThrowIfError(error, "Failed to get brake reverse");
        return result != 0;
    }

    /// <summary>
    /// Sets whether brake pedal output is reversed.
    /// </summary>
    public void SetBrakeReverse(bool reversed)
    {
        EnsureInitialized();
        var error = setPedalBrakeOutDir(reversed ? 1 : 0);
        ThrowIfError(error, "Failed to set brake reverse");
    }

    /// <summary>
    /// Toggles brake pedal reverse and returns the new state.
    /// </summary>
    public bool ToggleBrakeReverse()
    {
        var current = GetBrakeReverse();
        SetBrakeReverse(!current);
        return !current;
    }

    /// <summary>
    /// Gets whether clutch pedal output is reversed.
    /// </summary>
    public bool GetClutchReverse()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getPedalClutchOutDir(ref error);
        ThrowIfError(error, "Failed to get clutch reverse");
        return result != 0;
    }

    /// <summary>
    /// Sets whether clutch pedal output is reversed.
    /// </summary>
    public void SetClutchReverse(bool reversed)
    {
        EnsureInitialized();
        var error = setPedalClutchOutDir(reversed ? 1 : 0);
        ThrowIfError(error, "Failed to set clutch reverse");
    }

    /// <summary>
    /// Toggles clutch pedal reverse and returns the new state.
    /// </summary>
    public bool ToggleClutchReverse()
    {
        var current = GetClutchReverse();
        SetClutchReverse(!current);
        return !current;
    }

    /// <summary>
    /// Gets the handbrake application mode (0 = axis, 1 = button).
    /// </summary>
    public int GetHandbrakeMode()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getHandbrakeApplicationMode(ref error);
        ThrowIfError(error, "Failed to get handbrake mode");
        return result;
    }

    /// <summary>
    /// Sets the handbrake application mode (0 = axis, 1 = button).
    /// </summary>
    public void SetHandbrakeMode(int mode)
    {
        EnsureInitialized();
        var error = setHandbrakeApplicationMode(mode);
        ThrowIfError(error, "Failed to set handbrake mode");
    }

    /// <summary>
    /// Toggles handbrake between axis and button mode. Returns new mode (0 = axis, 1 = button).
    /// </summary>
    public int ToggleHandbrakeMode()
    {
        var current = GetHandbrakeMode();
        var newMode = current == 0 ? 1 : 0;
        SetHandbrakeMode(newMode);
        return newMode;
    }

    /// <summary>
    /// Gets whether auto-blip (automatic rev-match on downshift) is enabled.
    /// </summary>
    public bool GetAutoBlipEnabled()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getHandingShifterAutoBlipSwitch(ref error);
        ThrowIfError(error, "Failed to get auto-blip switch");
        return result != 0;
    }

    /// <summary>
    /// Sets whether auto-blip is enabled.
    /// </summary>
    public void SetAutoBlipEnabled(bool enabled)
    {
        EnsureInitialized();
        var error = setHandingShifterAutoBlipSwitch(enabled ? 1 : 0);
        ThrowIfError(error, "Failed to set auto-blip switch");
    }

    /// <summary>
    /// Toggles auto-blip and returns the new state.
    /// </summary>
    public bool ToggleAutoBlip()
    {
        var current = GetAutoBlipEnabled();
        SetAutoBlipEnabled(!current);
        return !current;
    }

    /// <summary>
    /// Gets the auto-blip output/throttle amount (0-100).
    /// </summary>
    public int GetAutoBlipOutput()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getHandingShifterAutoBlipOutput(ref error);
        ThrowIfError(error, "Failed to get auto-blip output");
        return result;
    }

    /// <summary>
    /// Sets the auto-blip output/throttle amount (0-100).
    /// </summary>
    public void SetAutoBlipOutput(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 0, 100);
        var error = setHandingShifterAutoBlipOutput(value);
        ThrowIfError(error, "Failed to set auto-blip output");
    }

    /// <summary>
    /// Adjusts auto-blip output by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustAutoBlipOutput(int delta)
    {
        var current = GetAutoBlipOutput();
        var newValue = Math.Clamp(current + delta, 0, 100);
        SetAutoBlipOutput(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the auto-blip duration (0-500).
    /// </summary>
    public int GetAutoBlipDuration()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getHandingShifterAutoBlipDuration(ref error);
        ThrowIfError(error, "Failed to get auto-blip duration");
        return result;
    }

    /// <summary>
    /// Sets the auto-blip duration (0-500).
    /// </summary>
    public void SetAutoBlipDuration(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 0, 500);
        var error = setHandingShifterAutoBlipDuration(value);
        ThrowIfError(error, "Failed to set auto-blip duration");
    }

    /// <summary>
    /// Adjusts auto-blip duration by a delta value, clamping to valid range.
    /// </summary>
    public int AdjustAutoBlipDuration(int delta)
    {
        var current = GetAutoBlipDuration();
        var newValue = Math.Clamp(current + delta, 0, 500);
        SetAutoBlipDuration(newValue);
        return newValue;
    }

    /// <summary>
    /// Gets the speed damping start point.
    /// </summary>
    public int GetSpeedDampingStartPoint()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorSpeedDampingStartPoint(ref error);
        ThrowIfError(error, "Failed to get speed damping start point");
        return result;
    }

    /// <summary>
    /// Sets the speed damping start point (1-200). SDK rejects 0; callers should skip the call for 0.
    /// Pit House presets store this as a speed value, not a percentage.
    /// </summary>
    public void SetSpeedDampingStartPoint(int value)
    {
        EnsureInitialized();
        value = Math.Clamp(value, 1, 200);
        var error = setMotorSpeedDampingStartPoint(value);
        ThrowIfError(error, "Failed to set speed damping start point");
    }

    /// <summary>
    /// Gets the hands-off protection mode.
    /// </summary>
    public int GetHandsOffProtection()
    {
        EnsureInitialized();
        ERRORCODE error = ERRORCODE.NORMAL;
        var result = getMotorHandsOffProtection(ref error);
        ThrowIfError(error, "Failed to get hands-off protection");
        return result;
    }

    /// <summary>
    /// Sets the hands-off protection mode.
    /// </summary>
    public void SetHandsOffProtection(int value)
    {
        EnsureInitialized();
        var error = setMotorHandsOffProtection(value);
        ThrowIfError(error, "Failed to set hands-off protection");
    }

    /// <summary>
    /// Applies all supported settings from a Pit House preset profile.
    /// Returns (applied, failed) counts. Continues past individual errors.
    /// </summary>
    public (int Applied, int Failed, List<string> Errors) ApplyPreset(PresetProfile preset)
    {
        EnsureInitialized();
        var applied = 0;
        var failed = 0;
        var errors = new List<string>();
        var p = preset.DeviceParams;

        void TryApply(string paramName, Action action)
        {
            if (!p.ContainsKey(paramName)) return;
            try { action(); applied++; }
            catch (Exception ex) { failed++; errors.Add($"{paramName}: {ex.Message}"); }
        }

        TryApply("gameForceFeedbackStrength", () =>
            SetFfbStrength(Convert.ToInt32(p["gameForceFeedbackStrength"])));

        // Handle steering angle — use separate game angle if available
        if (p.ContainsKey("maximumSteeringAngle"))
        {
            try
            {
                var hwAngle = Convert.ToInt32(p["maximumSteeringAngle"]);
                var gameAngle = p.TryGetValue("maximumGameSteeringAngle", out var ga)
                    ? Convert.ToInt32(ga) : hwAngle;
                SetWheelRotation(hwAngle, gameAngle);
                applied++;
            }
            catch (Exception ex) { failed++; errors.Add($"maximumSteeringAngle: {ex.Message}"); }
        }

        TryApply("maximumTorque", () =>
            SetMaxTorque(Convert.ToInt32(p["maximumTorque"])));

        TryApply("mechanicalDamper", () =>
            SetDamping(Convert.ToInt32(p["mechanicalDamper"])));

        TryApply("mechanicalSpringStrength", () =>
            SetSpringStrength(Convert.ToInt32(p["mechanicalSpringStrength"])));

        TryApply("mechanicalFriction", () =>
            SetNaturalFriction(Convert.ToInt32(p["mechanicalFriction"])));

        TryApply("maximumSteeringSpeed", () =>
            SetMaxWheelSpeed(Convert.ToInt32(p["maximumSteeringSpeed"])));

        TryApply("gameForceFeedbackReversal", () =>
        {
            var val = p["gameForceFeedbackReversal"];
            var reversed = val is bool b ? b : Convert.ToInt32(val) != 0;
            SetFfbReverse(reversed);
        });

        TryApply("speedDependentDamping", () =>
            SetSpeedDamping(Convert.ToInt32(p["speedDependentDamping"])));

        TryApply("initialSpeedDependentDamping", () =>
        {
            var startPoint = Convert.ToInt32(p["initialSpeedDependentDamping"]);
            if (startPoint > 0)
                SetSpeedDampingStartPoint(startPoint);
        });

        if (p.ContainsKey("safeDrivingEnabled") && p.ContainsKey("safeDrivingMode"))
        {
            try
            {
                var enabled = p["safeDrivingEnabled"] is bool sb ? sb : Convert.ToInt32(p["safeDrivingEnabled"]) != 0;
                var mode = enabled ? Math.Max(1, Convert.ToInt32(p["safeDrivingMode"])) : 0;
                try
                {
                    SetHandsOffProtection(mode);
                }
                catch (MozaException) when (mode > 1)
                {
                    // Some wheel bases don't support higher protection levels — fall back
                    SetHandsOffProtection(1);
                }
                applied++;
            }
            catch (Exception ex) { failed++; errors.Add($"safeDriving: {ex.Message}"); }
        }

        return (applied, failed, errors);
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("MozaDevice not initialized. Call Initialize() first.");
    }

    private static void ThrowIfError(ERRORCODE error, string message)
    {
        if (error != ERRORCODE.NORMAL)
            throw new MozaException($"{message}: {error}");
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (_initialized)
        {
            try
            {
                removeMozaSDK();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        _disposed = true;
        _initialized = false;
    }
}

/// <summary>
/// Exception thrown when a Moza SDK operation fails.
/// </summary>
public class MozaException : Exception
{
    public MozaException(string message) : base(message) { }
    public MozaException(string message, Exception innerException) : base(message, innerException) { }
}
