using TMPro;
using UnityEngine;

public class LLMResponsePanelController : MonoBehaviour
{
    public TMP_Text responseText;
    public float lifetime = 20f; // defaul close time for pop up description
    private float timer;

    public void SetText(string message)
    {
        responseText.text = message;
    }

    void Awake()
    {
        if (responseText == null)
        {
            responseText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }


    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            DestroySelf();
        }
    }

    public void ClosePanel() 
    {
        DestroySelf();
    }

    private void DestroySelf()
    {
        var spawner = FindObjectOfType<LLMResponseSpawner>();
        if (spawner != null)
        {
            spawner.ClearPanelReference();
        }

        Destroy(gameObject);
    }

}
