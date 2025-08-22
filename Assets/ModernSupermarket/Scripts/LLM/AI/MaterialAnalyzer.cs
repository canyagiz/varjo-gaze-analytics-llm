using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MaterialAnalyzer : MonoBehaviour
{
   

    [Header("Groq LLM Settings")]
    public string apiKey;
    public string endpoint = "https://api.groq.com/openai/v1/chat/completions";
    public string model = "llama-3.3-70b-versatile";
    public string systemPrompt = "You are a visual assistant helping users understand what a specific object in a 3D scene might be.\r\n\r\nYou will be provided:\r\n- The object's physical properties (name, surface area, volume)\r\n- A visual description based on a screenshot, including appearance and visible details\r\n\r\nYour task:\r\n- Focus only on the object that is directly in the center of the image\r\n- Use its physical size, shape, packaging, and any visible text or design to guess what it might be\r\n- Present your explanation as if you're describing it to a general user—not a developer\r\n\r\nDo not describe the surrounding environment. Do not mention other products or shelf labels.  \r\nAvoid quoting technical names. Focus only on what the centered object looks like and what it could be.\r\n\r\nLimit your response to a single, clear paragraph under 150 words.\r\n";
    public MonoBehaviour visualAnalyzerComponent;
    private ILLMVisualAnalyzer visualAnalyzer;

    void Awake()
    {
        visualAnalyzer = visualAnalyzerComponent as ILLMVisualAnalyzer;
        if (visualAnalyzer == null)
        {
            Debug.LogError("Assigned visualAnalyzerComponent does not implement ILLMVisualAnalyzer");
        }
    }

    public void AnalyzeMaterialWithVisual(MaterialData data, Renderer renderer)
    {
        if (visualAnalyzer == null)
        {
            Debug.LogWarning("[MaterialAnalyzer] VisualAnalyzer is not assigned or invalid.");
            return;
        }

        var spawner = Object.FindObjectOfType<LLMResponseSpawner>();
        if (spawner != null && spawner.HasActivePanel())
        {
            Debug.Log("[LLM Analyzer] Panel already exists. Skipping LLM call.");
            return;
        }

        StartCoroutine(visualAnalyzer.AnalyzeScene((visualInsight) => {AnalyzeMaterial(data, renderer, visualInsight);})); // calls AnalyzeMaterial via visualInsight ONLY IF Visual analyzing returns string succesfuly
    }

    public void AnalyzeMaterial(MaterialData data, Renderer renderer, string visualInsight)
    {
        string userPrompt = BuildPrompt(data, visualInsight);
        Debug.Log("[User Prompt] " + userPrompt);
        StartCoroutine(SendToLLM(systemPrompt, userPrompt, data, visualInsight, renderer));
    }

    private string BuildPrompt(MaterialData data, string visualInsight)
    {
        return $"GameObject Data:\n\n{data.ToString()}\n\nVisual Insight from Screenshot:\n{visualInsight}";
    }

    private IEnumerator SendToLLM(string systemPrompt, string userPrompt, MaterialData data, string visualInsight, Renderer renderer = null)
    {
        ChatRequest request = new ChatRequest
        {
            model = model,
            messages = new Message[]
            {
                new Message { role = "system", content = systemPrompt },
                new Message { role = "user", content = userPrompt }
            }
        };

        string json = JsonUtility.ToJson(request);

        using (UnityWebRequest req = new UnityWebRequest(endpoint, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string rawJson = req.downloadHandler.text;
                ChatResponse parsed = JsonUtility.FromJson<ChatResponse>(rawJson);

                if (parsed.choices != null && parsed.choices.Length > 0)
                {
                    string reply = parsed.choices[0].message.content.Trim();

                    LLMResponseSpawner spawner = Object.FindObjectOfType<LLMResponseSpawner>();
                    if (spawner != null)
                    {
                        spawner.ShowLLMResponse(reply, renderer);
                    }

                    LLMLogEntry log = new LLMLogEntry
                    {
                        sessionId = SessionManager.GetSessionId(),
                        objectName = data.objectName,
                        area = data.surfaceArea,
                        volume = data.volume,
                        visualInsight = visualInsight,
                        llmReply = reply,
                        timestamp = System.DateTime.UtcNow.ToString("o")
                    };

                    LLMDatabase.InsertLog(log);
                    Debug.Log("[LLM Assistant Reply] " + reply);
                }
            }
            else
            {
                Debug.LogError("LLM request failed: " + req.error);
            }
        }
    }

    [System.Serializable]
    private class ChatRequest
    {
        public string model;
        public Message[] messages;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class ChatResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }
}
