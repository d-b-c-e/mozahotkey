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
        return $"Road Sens: {newValue}/10";
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
        return $"Road Sens: {newValue}/10";
    }
}

/// <summary>
/// Action that increases speed damping by a specified amount.
/// </summary>
public class IncreaseSpeedDampingAction : MozaAction
{
    private readonly int _amount;

    public IncreaseSpeedDampingAction(int amount = 5)
        : base($"speed_damping_increase_{amount}", $"Increase Speed Damp +{amount}", $"Increases speed damping by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustSpeedDamping(_amount);
        return $"Speed Damp: {newValue}%";
    }
}

/// <summary>
/// Action that decreases speed damping by a specified amount.
/// </summary>
public class DecreaseSpeedDampingAction : MozaAction
{
    private readonly int _amount;

    public DecreaseSpeedDampingAction(int amount = 5)
        : base($"speed_damping_decrease_{amount}", $"Decrease Speed Damp -{amount}", $"Decreases speed damping by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustSpeedDamping(-_amount);
        return $"Speed Damp: {newValue}%";
    }
}

/// <summary>
/// Action that toggles FFB reverse.
/// </summary>
public class ToggleFfbReverseAction : MozaAction
{
    public ToggleFfbReverseAction()
        : base("ffb_reverse_toggle", "Toggle FFB Reverse", "Toggles Force Feedback direction")
    {
    }

    public override string Execute(MozaDevice device)
    {
        var newState = device.ToggleFfbReverse();
        return newState ? "FFB: Reversed" : "FFB: Normal";
    }
}

/// <summary>
/// Action that stops all force feedback.
/// </summary>
public class StopFfbAction : MozaAction
{
    public StopFfbAction()
        : base("stop_ffb", "Stop FFB", "Emergency stop - kills all force feedback")
    {
    }

    public override string Execute(MozaDevice device)
    {
        device.StopForceFeedback();
        return "FFB Stopped";
    }
}

/// <summary>
/// Action that toggles throttle pedal reverse.
/// </summary>
public class ToggleThrottleReverseAction : MozaAction
{
    public ToggleThrottleReverseAction()
        : base("throttle_reverse_toggle", "Toggle Throttle Reverse", "Toggles throttle pedal output direction")
    {
    }

    public override string Execute(MozaDevice device)
    {
        var newState = device.ToggleThrottleReverse();
        return newState ? "Throttle: Reversed" : "Throttle: Normal";
    }
}

/// <summary>
/// Action that toggles brake pedal reverse.
/// </summary>
public class ToggleBrakeReverseAction : MozaAction
{
    public ToggleBrakeReverseAction()
        : base("brake_reverse_toggle", "Toggle Brake Reverse", "Toggles brake pedal output direction")
    {
    }

    public override string Execute(MozaDevice device)
    {
        var newState = device.ToggleBrakeReverse();
        return newState ? "Brake: Reversed" : "Brake: Normal";
    }
}

/// <summary>
/// Action that toggles clutch pedal reverse.
/// </summary>
public class ToggleClutchReverseAction : MozaAction
{
    public ToggleClutchReverseAction()
        : base("clutch_reverse_toggle", "Toggle Clutch Reverse", "Toggles clutch pedal output direction")
    {
    }

    public override string Execute(MozaDevice device)
    {
        var newState = device.ToggleClutchReverse();
        return newState ? "Clutch: Reversed" : "Clutch: Normal";
    }
}

/// <summary>
/// Action that toggles handbrake between axis and button mode.
/// </summary>
public class ToggleHandbrakeModeAction : MozaAction
{
    public ToggleHandbrakeModeAction()
        : base("handbrake_mode_toggle", "Toggle Handbrake Mode", "Toggles handbrake between axis and button mode")
    {
    }

    public override string Execute(MozaDevice device)
    {
        var newMode = device.ToggleHandbrakeMode();
        return newMode == 0 ? "Handbrake: Axis" : "Handbrake: Button";
    }
}

/// <summary>
/// Action that toggles auto-blip (automatic rev-match on downshift).
/// </summary>
public class ToggleAutoBlipAction : MozaAction
{
    public ToggleAutoBlipAction()
        : base("auto_blip_toggle", "Toggle Auto-Blip", "Toggles automatic rev-match on downshift")
    {
    }

    public override string Execute(MozaDevice device)
    {
        var newState = device.ToggleAutoBlip();
        return newState ? "Auto-Blip: ON" : "Auto-Blip: OFF";
    }
}

/// <summary>
/// Action that increases auto-blip output by a specified amount.
/// </summary>
public class IncreaseAutoBlipOutputAction : MozaAction
{
    private readonly int _amount;

    public IncreaseAutoBlipOutputAction(int amount = 5)
        : base($"auto_blip_output_increase_{amount}", $"Increase Blip Output +{amount}", $"Increases auto-blip throttle output by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustAutoBlipOutput(_amount);
        return $"Blip Output: {newValue}%";
    }
}

/// <summary>
/// Action that decreases auto-blip output by a specified amount.
/// </summary>
public class DecreaseAutoBlipOutputAction : MozaAction
{
    private readonly int _amount;

    public DecreaseAutoBlipOutputAction(int amount = 5)
        : base($"auto_blip_output_decrease_{amount}", $"Decrease Blip Output -{amount}", $"Decreases auto-blip throttle output by {amount}")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustAutoBlipOutput(-_amount);
        return $"Blip Output: {newValue}%";
    }
}

/// <summary>
/// Action that increases auto-blip duration by a specified amount.
/// </summary>
public class IncreaseAutoBlipDurationAction : MozaAction
{
    private readonly int _amount;

    public IncreaseAutoBlipDurationAction(int amount = 50)
        : base($"auto_blip_duration_increase_{amount}", $"Increase Blip Duration +{amount}", $"Increases auto-blip duration by {amount}ms")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustAutoBlipDuration(_amount);
        return $"Blip Duration: {newValue}ms";
    }
}

/// <summary>
/// Action that decreases auto-blip duration by a specified amount.
/// </summary>
public class DecreaseAutoBlipDurationAction : MozaAction
{
    private readonly int _amount;

    public DecreaseAutoBlipDurationAction(int amount = 50)
        : base($"auto_blip_duration_decrease_{amount}", $"Decrease Blip Duration -{amount}", $"Decreases auto-blip duration by {amount}ms")
    {
        _amount = amount;
    }

    public override string Execute(MozaDevice device)
    {
        var newValue = device.AdjustAutoBlipDuration(-_amount);
        return $"Blip Duration: {newValue}ms";
    }
}
