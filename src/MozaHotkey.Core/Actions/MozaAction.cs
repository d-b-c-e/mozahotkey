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
