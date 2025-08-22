using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerLookSource : MonoBehaviour, ILookSource
{
    public Transform controllerTransform;

    [Header("Trigger Input (Input System)")]
    public InputActionProperty triggerAction; // Input Action referencing the trigger

    void Awake()
    {
        if (controllerTransform == null)
            controllerTransform = this.transform;
    }

    public bool TryGetLookRay(out Ray ray)
    {
        // Default to an invalid ray if trigger is not pressed
        ray = default;

        if (triggerAction != null && triggerAction.action != null && triggerAction.action.ReadValue<float>() > 0.1f)
        {
            ray = new Ray(controllerTransform.position, controllerTransform.forward);
            return true; // Trigger pressed
        }

        return false; // Trigger not pressed
    }

}
