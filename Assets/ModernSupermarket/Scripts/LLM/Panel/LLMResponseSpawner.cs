using UnityEngine;

public class LLMResponseSpawner : MonoBehaviour
{
    public GameObject llmPanelPrefab;

    [Range(0.0f, 1.0f)]
    public float distance = 0.5f;
    
    private GameObject currentPanel;

    public void ShowLLMResponse(string message, Renderer renderer)
    {
        if (currentPanel != null)
        {
            Debug.Log("[LLMResponseSpawner] Panel already exists. Skipping instantiation.");
            return;
        }

        Vector3 center = renderer.bounds.center;
       
        Vector3 spawnPos = Vector3.Lerp(center, Camera.main.transform.position, distance);

        currentPanel = Instantiate(llmPanelPrefab, spawnPos, Quaternion.identity);

        Vector3 lookDir = (Camera.main.transform.position - currentPanel.transform.position).normalized;
        currentPanel.transform.rotation = Quaternion.LookRotation(-lookDir);

        var controller = currentPanel.GetComponent<LLMResponsePanelController>();
        if (controller != null)
        {
            controller.SetText(message);
        }
    }

    public bool HasActivePanel()
    {
        return currentPanel != null;
    }

    public void ClearPanelReference()
    {
        currentPanel = null;
    }
}
