namespace MozaHotkey.Core.Actions;

/// <summary>
/// Represents an action that can be triggered by a hotkey.
/// </summary>
public abstract class MozaAction
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    protected MozaAction(string id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Executes the action and returns a status message.
    /// </summary>
    public abstract string Execute(MozaDevice device);
}

/// <summary>
/// Action that increases FFB strength by a specified amount.
/// </summary>
public class IncreaseFfbAction : MozaAction
{
    private readonly int _amount;

    public IncreaseFfbAction(int amount = 5)
        : base($"ffb_increase_{amount}", $"Increase FFB +{amount}", $"Increases Force Feedback strength by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustFfbStrength(_amount);
        return $"FFB: {newValue}%";
    }
}

/// <summary>
/// Action that decreases FFB strength by a specified amount.
/// </summary>
public class DecreaseFfbAction : MozaAction
{
    private readonly int _amount;

    public DecreaseFfbAction(int amount = 5)
        : base($"ffb_decrease_{amount}", $"Decrease FFB -{amount}", $"Decreases Force Feedback strength by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustFfbStrength(-_amount);
        return $"FFB: {newValue}%";
    }
}

/// <summary>
/// Action that increases wheel rotation by a specified amount.
/// </summary>
public class IncreaseRotationAction : MozaAction
{
    private readonly int _amount;

    public IncreaseRotationAction(int amount = 90)
        : base($"rotation_increase_{amount}", $"Increase Rotation +{amount}", $"Increases wheel rotation by {amount} degrees")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustWheelRotation(_amount);
        return $"Rotation: {newValue}°";
    }
}

/// <summary>
/// Action that decreases wheel rotation by a specified amount.
/// </summary>
public class DecreaseRotationAction : MozaAction
{
    private readonly int _amount;

    public DecreaseRotationAction(int amount = 90)
        : base($"rotation_decrease_{amount}", $"Decrease Rotation -{amount}", $"Decreases wheel rotation by {amount} degrees")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustWheelRotation(-_amount);
        return $"Rotation: {newValue}°";
    }
}

/// <summary>
/// Action that sets wheel rotation to a specific value.
/// </summary>
public class SetRotationAction : MozaAction
{
    private readonly int _degrees;

    public SetRotationAction(int degrees)
        : base($"rotation_set_{degrees}", $"Set Rotation {degrees}°", $"Sets wheel rotation to {degrees} degrees")
    {
        _degrees = degrees;
    }

    public override string Execute(MozaDevice device)
    {
        device.SetWheelRotation(_degrees);
        return $"Rotation: {_degrees}°";
    }
}

/// <summary>
/// Action that centers the wheel.
/// </summary>
public class CenterWheelAction : MozaAction
{
    public CenterWheelAction()
        : base("center_wheel", "Center Wheel", "Centers the steering wheel")
    {
    }

    public override string Execute(MozaDevice device)
    {
        device.CenterWheel();
        return "Wheel Centered";
    }
}

/// <summary>
/// Action that increases damping by a specified amount.
/// </summary>
public class IncreaseDampingAction : MozaAction
{
    private readonly int _amount;

    public IncreaseDampingAction(int amount = 5)
        : base($"damping_increase_{amount}", $"Increase Damping +{amount}", $"Increases damping (road feel) by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustDamping(_amount);
        return $"Damping: {newValue}%";
    }
}

/// <summary>
/// Action that decreases damping by a specified amount.
/// </summary>
public class DecreaseDampingAction : MozaAction
{
    private readonly int _amount;

    public DecreaseDampingAction(int amount = 5)
        : base($"damping_decrease_{amount}", $"Decrease Damping -{amount}", $"Decreases damping (road feel) by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustDamping(-_amount);
        return $"Damping: {newValue}%";
    }
}

/// <summary>
/// Action that increases road sensitivity by a specified amount.
/// </summary>
public class IncreaseRoadSensitivityAction : MozaAction
{
    private readonly int _amount;

    public IncreaseRoadSensitivityAction(int amount = 1)
        : base($"road_sensitivity_increase_{amount}", $"Increase Road Sens +{amount}", $"Increases road sensitivity by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustRoadSensitivity(_amount);
        return $"Road Sensitivity: {newValue}";
    }
}

/// <summary>
/// Action that decreases road sensitivity by a specified amount.
/// </summary>
public class DecreaseRoadSensitivityAction : MozaAction
{
    private readonly int _amount;

    public DecreaseRoadSensitivityAction(int amount = 1)
        : base($"road_sensitivity_decrease_{amount}", $"Decrease Road Sens -{amount}", $"Decreases road sensitivity by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustRoadSensitivity(-_amount);
        return $"Road Sensitivity: {newValue}";
    }
}

/// <summary>
/// Action that increases max torque by a specified amount.
/// </summary>
public class IncreaseMaxTorqueAction : MozaAction
{
    private readonly int _amount;

    public IncreaseMaxTorqueAction(int amount = 5)
        : base($"max_torque_increase_{amount}", $"Increase Max Torque +{amount}", $"Increases max torque limit by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustMaxTorque(_amount);
        return $"Max Torque: {newValue}%";
    }
}

/// <summary>
/// Action that decreases max torque by a specified amount.
/// </summary>
public class DecreaseMaxTorqueAction : MozaAction
{
    private readonly int _amount;

    public DecreaseMaxTorqueAction(int amount = 5)
        : base($"max_torque_decrease_{amount}", $"Decrease Max Torque -{amount}", $"Decreases max torque limit by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustMaxTorque(-_amount);
        return $"Max Torque: {newValue}%";
    }
}
