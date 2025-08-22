using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LookableObject : MonoBehaviour
{
    private float lookTimer = 0f;
    public float threshold = 3f;
    private bool hasPrinted = false;

    private MaterialAnalyzer analyzer; // Reference to the analyzer component

    void Start()
    {
        // Try to find the MaterialAnalyzer in the scene (could also be set manually)
        analyzer = Object.FindObjectOfType<MaterialAnalyzer>();
        if (analyzer == null)
        {
            Debug.LogWarning("MaterialAnalyzer not found in the scene.");
        }
    }

    public void OnLook(float deltaTime)
    {
        if (hasPrinted || analyzer == null) return;

        lookTimer += deltaTime;

        if (lookTimer >= threshold)
        {
            Debug.Log($"[Look Detected] Object: {gameObject.name}");

            Renderer rend = GetComponent<Renderer>();
            MaterialData matData = MaterialData.FromRenderer(rend);

            analyzer.AnalyzeMaterialWithVisual(matData, rend);

            hasPrinted = true;
        }
    }


    public void ResetFocus()
    {
        lookTimer = 0f;
        hasPrinted = false;
    }
}
