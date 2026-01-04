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
        _actions.Add(new SetRotationAction(270));
        _actions.Add(new SetRotationAction(360));
        _actions.Add(new SetRotationAction(540));
        _actions.Add(new SetRotationAction(720));
        _actions.Add(new SetRotationAction(900));
        _actions.Add(new SetRotationAction(1080));

        // Damping Actions
        _actions.Add(new IncreaseDampingAction(5));
        _actions.Add(new DecreaseDampingAction(5));
        _actions.Add(new IncreaseDampingAction(10));
        _actions.Add(new DecreaseDampingAction(10));

        // Road Sensitivity Actions (range 0-10, so smaller increments)
        _actions.Add(new IncreaseRoadSensitivityAction(1));
        _actions.Add(new DecreaseRoadSensitivityAction(1));
        _actions.Add(new IncreaseRoadSensitivityAction(2));
        _actions.Add(new DecreaseRoadSensitivityAction(2));

        // Max Torque Actions
        _actions.Add(new IncreaseMaxTorqueAction(5));
        _actions.Add(new DecreaseMaxTorqueAction(5));
        _actions.Add(new IncreaseMaxTorqueAction(10));
        _actions.Add(new DecreaseMaxTorqueAction(10));

        // Utility
        _actions.Add(new CenterWheelAction());
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
