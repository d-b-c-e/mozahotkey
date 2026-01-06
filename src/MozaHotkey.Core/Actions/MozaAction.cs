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
/// Action that increases steering wheel inertia by a specified amount.
/// </summary>
public class IncreaseSteeringWheelInertiaAction : MozaAction
{
    private readonly int _amount;

    public IncreaseSteeringWheelInertiaAction(int amount = 50)
        : base($"sw_inertia_increase_{amount}", $"Increase SW Inertia +{amount}", $"Increases steering wheel inertia by {amount}g")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustSteeringWheelInertia(_amount);
        return $"SW Inertia: {newValue}g";
    }
}

/// <summary>
/// Action that decreases steering wheel inertia by a specified amount.
/// </summary>
public class DecreaseSteeringWheelInertiaAction : MozaAction
{
    private readonly int _amount;

    public DecreaseSteeringWheelInertiaAction(int amount = 50)
        : base($"sw_inertia_decrease_{amount}", $"Decrease SW Inertia -{amount}", $"Decreases steering wheel inertia by {amount}g")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustSteeringWheelInertia(-_amount);
        return $"SW Inertia: {newValue}g";
    }
}

/// <summary>
/// Action that increases max wheel speed by a specified amount.
/// </summary>
public class IncreaseMaxWheelSpeedAction : MozaAction
{
    private readonly int _amount;

    public IncreaseMaxWheelSpeedAction(int amount = 5)
        : base($"max_wheel_speed_increase_{amount}", $"Increase Max Speed +{amount}", $"Increases max wheel speed by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustMaxWheelSpeed(_amount);
        return $"Max Speed: {newValue}%";
    }
}

/// <summary>
/// Action that decreases max wheel speed by a specified amount.
/// </summary>
public class DecreaseMaxWheelSpeedAction : MozaAction
{
    private readonly int _amount;

    public DecreaseMaxWheelSpeedAction(int amount = 5)
        : base($"max_wheel_speed_decrease_{amount}", $"Decrease Max Speed -{amount}", $"Decreases max wheel speed by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustMaxWheelSpeed(-_amount);
        return $"Max Speed: {newValue}%";
    }
}

/// <summary>
/// Action that increases natural friction by a specified amount.
/// </summary>
public class IncreaseNaturalFrictionAction : MozaAction
{
    private readonly int _amount;

    public IncreaseNaturalFrictionAction(int amount = 5)
        : base($"natural_friction_increase_{amount}", $"Increase Friction +{amount}", $"Increases natural friction by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustNaturalFriction(_amount);
        return $"Friction: {newValue}%";
    }
}

/// <summary>
/// Action that decreases natural friction by a specified amount.
/// </summary>
public class DecreaseNaturalFrictionAction : MozaAction
{
    private readonly int _amount;

    public DecreaseNaturalFrictionAction(int amount = 5)
        : base($"natural_friction_decrease_{amount}", $"Decrease Friction -{amount}", $"Decreases natural friction by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustNaturalFriction(-_amount);
        return $"Friction: {newValue}%";
    }
}

/// <summary>
/// Action that increases natural inertia by a specified amount.
/// </summary>
public class IncreaseNaturalInertiaAction : MozaAction
{
    private readonly int _amount;

    public IncreaseNaturalInertiaAction(int amount = 5)
        : base($"natural_inertia_increase_{amount}", $"Increase Inertia +{amount}", $"Increases natural inertia by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustNaturalInertia(_amount);
        return $"Inertia: {newValue}%";
    }
}

/// <summary>
/// Action that decreases natural inertia by a specified amount.
/// </summary>
public class DecreaseNaturalInertiaAction : MozaAction
{
    private readonly int _amount;

    public DecreaseNaturalInertiaAction(int amount = 5)
        : base($"natural_inertia_decrease_{amount}", $"Decrease Inertia -{amount}", $"Decreases natural inertia by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustNaturalInertia(-_amount);
        return $"Inertia: {newValue}%";
    }
}

/// <summary>
/// Action that increases spring strength by a specified amount.
/// </summary>
public class IncreaseSpringStrengthAction : MozaAction
{
    private readonly int _amount;

    public IncreaseSpringStrengthAction(int amount = 5)
        : base($"spring_strength_increase_{amount}", $"Increase Spring +{amount}", $"Increases spring strength by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustSpringStrength(_amount);
        return $"Spring: {newValue}%";
    }
}

/// <summary>
/// Action that decreases spring strength by a specified amount.
/// </summary>
public class DecreaseSpringStrengthAction : MozaAction
{
    private readonly int _amount;

    public DecreaseSpringStrengthAction(int amount = 5)
        : base($"spring_strength_decrease_{amount}", $"Decrease Spring -{amount}", $"Decreases spring strength by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustSpringStrength(-_amount);
        return $"Spring: {newValue}%";
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
