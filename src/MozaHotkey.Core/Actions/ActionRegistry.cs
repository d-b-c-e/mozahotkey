namespace MozaHotkey.Core.Actions;

/// <summary>
/// Registry of all available Moza actions.
/// </summary>
public static class ActionRegistry
{
    private static readonly List<MozaAction> _actions = new();

    static ActionRegistry()
    {
        // FFB Actions
        _actions.Add(new IncreaseFfbAction(5));
        _actions.Add(new DecreaseFfbAction(5));
        _actions.Add(new IncreaseFfbAction(10));
        _actions.Add(new DecreaseFfbAction(10));

        // Rotation Adjustments
        _actions.Add(new IncreaseRotationAction(90));
        _actions.Add(new DecreaseRotationAction(90));
        _actions.Add(new IncreaseRotationAction(180));
        _actions.Add(new DecreaseRotationAction(180));

        // Preset Rotations
        _actions.Add(new SetRotationAction(180));
        _actions.Add(new SetRotationAction(270));
        _actions.Add(new SetRotationAction(360));
        _actions.Add(new SetRotationAction(540));
        _actions.Add(new SetRotationAction(720));
        _actions.Add(new SetRotationAction(900));
        _actions.Add(new SetRotationAction(1080));

        // Damping Actions (Natural Dampening)
        _actions.Add(new IncreaseDampingAction(5));
        _actions.Add(new DecreaseDampingAction(5));
        _actions.Add(new IncreaseDampingAction(10));
        _actions.Add(new DecreaseDampingAction(10));

        // Max Torque Actions
        _actions.Add(new IncreaseMaxTorqueAction(5));
        _actions.Add(new DecreaseMaxTorqueAction(5));
        _actions.Add(new IncreaseMaxTorqueAction(10));
        _actions.Add(new DecreaseMaxTorqueAction(10));

        // Steering Wheel Inertia Actions (range 100-1550g)
        _actions.Add(new IncreaseSteeringWheelInertiaAction(50));
        _actions.Add(new DecreaseSteeringWheelInertiaAction(50));
        _actions.Add(new IncreaseSteeringWheelInertiaAction(100));
        _actions.Add(new DecreaseSteeringWheelInertiaAction(100));

        // Max Wheel Speed Actions
        _actions.Add(new IncreaseMaxWheelSpeedAction(5));
        _actions.Add(new DecreaseMaxWheelSpeedAction(5));
        _actions.Add(new IncreaseMaxWheelSpeedAction(10));
        _actions.Add(new DecreaseMaxWheelSpeedAction(10));

        // Natural Friction Actions
        _actions.Add(new IncreaseNaturalFrictionAction(5));
        _actions.Add(new DecreaseNaturalFrictionAction(5));
        _actions.Add(new IncreaseNaturalFrictionAction(10));
        _actions.Add(new DecreaseNaturalFrictionAction(10));

        // Natural Inertia Actions
        _actions.Add(new IncreaseNaturalInertiaAction(5));
        _actions.Add(new DecreaseNaturalInertiaAction(5));
        _actions.Add(new IncreaseNaturalInertiaAction(10));
        _actions.Add(new DecreaseNaturalInertiaAction(10));

        // Spring Strength Actions
        _actions.Add(new IncreaseSpringStrengthAction(5));
        _actions.Add(new DecreaseSpringStrengthAction(5));
        _actions.Add(new IncreaseSpringStrengthAction(10));
        _actions.Add(new DecreaseSpringStrengthAction(10));

        // Road Sensitivity Actions (0-10)
        _actions.Add(new IncreaseRoadSensitivityAction(1));
        _actions.Add(new DecreaseRoadSensitivityAction(1));
        _actions.Add(new IncreaseRoadSensitivityAction(2));
        _actions.Add(new DecreaseRoadSensitivityAction(2));

        // Speed Damping Actions
        _actions.Add(new IncreaseSpeedDampingAction(5));
        _actions.Add(new DecreaseSpeedDampingAction(5));
        _actions.Add(new IncreaseSpeedDampingAction(10));
        _actions.Add(new DecreaseSpeedDampingAction(10));

        // Utility
        _actions.Add(new CenterWheelAction());
        _actions.Add(new ToggleFfbReverseAction());
        _actions.Add(new StopFfbAction());

        // Pedal Reverse Toggles
        _actions.Add(new ToggleThrottleReverseAction());
        _actions.Add(new ToggleBrakeReverseAction());
        _actions.Add(new ToggleClutchReverseAction());

        // Handbrake Mode Toggle
        _actions.Add(new ToggleHandbrakeModeAction());
    }

    /// <summary>
    /// Gets all available actions.
    /// </summary>
    public static IReadOnlyList<MozaAction> GetAllActions() => _actions.AsReadOnly();

    /// <summary>
    /// Gets an action by its ID.
    /// </summary>
    public static MozaAction? GetAction(string id) => _actions.FirstOrDefault(a => a.Id == id);

    /// <summary>
    /// Registers a custom action.
    /// </summary>
    public static void RegisterAction(MozaAction action)
    {
        if (_actions.Any(a => a.Id == action.Id))
            throw new ArgumentException($"Action with ID '{action.Id}' already exists.");

        _actions.Add(action);
    }
}
