using UnityEngine;

public class LookTracker : MonoBehaviour
{
    public MonoBehaviour lookSourceBehaviour; // Must implement ILookSource
    private ILookSource lookSource;

    [Header("Analyzer")]
    public MonoBehaviour visualAnalyzerBehaviour; // Must implement ILLMVisualAnalyzer
    private ILLMVisualAnalyzer analyzer;

    private LookableObject currentLookedObject;
    private bool isTrackingEnabled = true;

    [Header("Range of track")]
    public float rangeOfTrack =5f;

    [Header("Tracking Toggle Key")]
    public KeyCode toggleKey = KeyCode.L;

    void Start()
    {
        lookSource = lookSourceBehaviour as ILookSource;
        if (lookSource == null)
            Debug.LogError("Assigned LookSource does not implement ILookSource!");

        analyzer = visualAnalyzerBehaviour as ILLMVisualAnalyzer;
        if (analyzer == null)
            Debug.LogError("Assigned Analyzer does not implement ILLMVisualAnalyzer!");
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isTrackingEnabled = !isTrackingEnabled;
            Debug.Log("Look tracking is now " + (isTrackingEnabled ? "ENABLED" : "DISABLED"));
        }

        if (!isTrackingEnabled || lookSource == null)
        {
            ResetCurrent();
            return;
        }

        if (lookSource.TryGetLookRay(out Ray ray))
        {
            if (Physics.Raycast(ray, out RaycastHit hit, rangeOfTrack))
            {
                LookableObject lookable = hit.collider.GetComponent<LookableObject>();
                if (lookable != null)
                {
                    if (lookable != currentLookedObject)
                    {
                        ResetCurrent();
                        currentLookedObject = lookable;
                        // Target'ı dinamik ata:
                        analyzer.SetTargetObject(lookable);

                        // NEW: ray yönünü kamera yönü olarak ilet
                        if (analyzer is ILLMVisualAnalyzer visualWithView)
                        {
                            visualWithView.SetViewDirection(ray.direction);
                        }
                    }
                    currentLookedObject.OnLook(Time.deltaTime);
                }
                else
                {
                    ResetCurrent();
                }
            }
            else
            {
                ResetCurrent();
            }
        }
    }

    void ResetCurrent()
    {
        if (currentLookedObject != null)
        {
            currentLookedObject.ResetFocus();
            currentLookedObject = null;
            analyzer?.SetTargetObject(null); // Target temizle
        }
    }
}
