using mozaAPI;
using static mozaAPI.mozaAPI;

namespace MozaHotkey.Core;

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
    /// Centers the wheel.
    /// </summary>
    public void CenterWheel()
    {
        EnsureInitialized();
        var error = mozaAPI.mozaAPI.CenterWheel();
        ThrowIfError(error, "Failed to center wheel");
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
