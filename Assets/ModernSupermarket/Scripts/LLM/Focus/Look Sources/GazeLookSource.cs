using UnityEngine;
using Varjo.XR;

public class GazeLookSource : MonoBehaviour, ILookSource
{
    [Tooltip("Head transform (camera root) to convert Varjo local gaze vectors to world space.")]
    public Transform headTransform;

    public bool TryGetLookRay(out Ray ray)
    {
        ray = default;

        if (headTransform == null)
        {
            Debug.LogWarning("[GazeLookSource] Head transform is not assigned.");
            return false;
        }

        if (Three.VarjoIntegration.VarjoGazeMonitor.Instance == null)
        {
            Debug.LogWarning("[GazeLookSource] VarjoGazeMonitor instance not found.");
            return false;
        }

        var gazeData = Three.VarjoIntegration.VarjoGazeMonitor.Instance.GetCurrentGaze();

        if (gazeData.leftStatus == VarjoEyeTracking.GazeEyeStatus.Invalid ||
            gazeData.rightStatus == VarjoEyeTracking.GazeEyeStatus.Invalid)
        {
            return false;
        }

        // GAZE TO RAY
        Vector3 origin = headTransform.TransformPoint(gazeData.gaze.origin);
        Vector3 direction = headTransform.TransformDirection(gazeData.gaze.forward).normalized;

        ray = new Ray(origin, direction);

        //  Debug.Log($"[GazeLookSource] Gaze ray created: Origin = {origin}, Direction = {direction}");

        return true;
    }
}
