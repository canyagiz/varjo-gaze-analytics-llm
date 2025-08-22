using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VisualAnalyzerOpenAI : MonoBehaviour, ILLMVisualAnalyzer
{
    [Header("OpenAI Settings")]
    public string apiKey;
    public string endpoint = "https://api.openai.com/v1/chat/completions";
    [TextArea(5, 10)]
    public string systemPrompt = "You will be shown a screenshot from a 3D retail scene...\n\n1. Briefly describe the overall environment...\n2. Then, carefully analyze the object that is directly centered...";

    [Header("Capture Settings")]
    public LookableObject targetObject;
    public ScreenshotHelper.CaptureSettings captureSettings = new ScreenshotHelper.CaptureSettings();

    public string ServiceName => "OpenAI";

    public void SetTargetObject(LookableObject lookable)
    {
        targetObject = lookable;
    }

    private Vector3 viewDirection = Vector3.zero;

    public void SetViewDirection(Vector3 direction)
    {
        viewDirection = direction.normalized;
    }

    public IEnumerator AnalyzeScene(Action<string> onComplete)
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object not assigned.");
            onComplete?.Invoke("Target object not assigned.");
            yield break;
        }

        yield return new WaitForEndOfFrame();

        string base64Image = ScreenshotHelper.CaptureObjectScreenshot(targetObject, captureSettings, viewDirection);

        if (string.IsNullOrEmpty(base64Image))
        {
            onComplete?.Invoke("Failed to capture screenshot.");
            yield break;
        }
        string imageUrl = $"data:image/jpeg;base64,{base64Image}";

        var contentList = new List<object>
        {
            new Dictionary<string, object> { { "type", "text" }, { "text", $"{systemPrompt}"  } },
            new Dictionary<string, object> { { "type", "image_url" }, { "image_url", new Dictionary<string, string> { { "url", imageUrl } } } }
        };

        var message = new Dictionary<string, object> { { "role", "user" }, { "content", contentList } };
        var requestData = new Dictionary<string, object>
        {
            { "model", "gpt-4o" },
            { "messages", new List<object> { message } },
            { "max_tokens", 300 }
        };

        string jsonBody = JsonConvert.SerializeObject(requestData);

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<ChatResponse>(request.downloadHandler.text);
                string reply = response?.choices?[0]?.message?.content?.Trim();
                onComplete?.Invoke(reply ?? "No description returned.");
            }
            catch (Exception ex)
            {
                Debug.LogError("JSON parse error: " + ex.Message);
                onComplete?.Invoke("Error parsing response.");
            }
        }
        else
        {
            Debug.LogError("OpenAI API error: " + request.error);
            onComplete?.Invoke("Request failed.");
        }
    }

    [Serializable]
    public class ChatResponse
    {
        public List<Choice> choices;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }
}
