using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VisualAnalyzerGemini : MonoBehaviour, ILLMVisualAnalyzer
{
    [Header("Gemini API Settings")]
    public string apiKey;
    public string endpoint = "...";
    [TextArea(5, 10)]
    public string systemPrompt = "...";

    private LookableObject targetObject;

    public ScreenshotHelper.CaptureSettings captureSettings = new ScreenshotHelper.CaptureSettings();

    public string ServiceName => "Gemini";

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

        string base64Image = ScreenshotHelper.CaptureObjectScreenshot(
            targetObject, captureSettings, viewDirection
        );

        if (string.IsNullOrEmpty(base64Image))
        {
            onComplete?.Invoke("Failed to capture screenshot.");
            yield break;
        }

        var requestData = new
        {
            contents = new[] {
                new {
                    parts = new object[] {
                        new { text = $"{systemPrompt}" },
                        new { inline_data = new { mime_type = "image/jpeg", data = base64Image } }
                    }
                }
            }
        };

        string jsonBody = JsonConvert.SerializeObject(requestData);

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-goog-api-key", apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var parsed = JsonConvert.DeserializeObject<GeminiResponse>(request.downloadHandler.text);
                string reply = parsed?.candidates?[0]?.content?.parts?[0]?.text?.Trim();
                onComplete?.Invoke(reply ?? "No response.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse response: " + ex.Message);
                onComplete?.Invoke("Error parsing response.");
            }
        }
        else
        {
            Debug.LogError("Gemini API error: " + request.error);
            onComplete?.Invoke("Request failed.");
        }
    }

    public void SetTargetObject(LookableObject lookable)
    {
        targetObject = lookable;
    }

    [Serializable]
    public class GeminiResponse
    {
        public List<Candidate> candidates;
    }

    [Serializable]
    public class Candidate
    {
        public Content content;
    }

    [Serializable]
    public class Content
    {
        public List<Part> parts;
    }

    [Serializable]
    public class Part
    {
        public string text;
    }
}
