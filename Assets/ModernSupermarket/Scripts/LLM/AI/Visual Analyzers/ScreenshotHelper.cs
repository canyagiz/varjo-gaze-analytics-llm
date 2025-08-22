using System;
using System.IO;
using UnityEngine;

public static class ScreenshotHelper
{
    [Serializable]
    public class CaptureSettings
    {
        public int width = 512; // Screenshot width in pixels
        public int height = 512; // Screenshot height in pixels
        [Range(0, 100)] public int resolution = 100; // JPG quality (0–100)
        public float fieldOfView = 40f; // Camera FOV in degrees
        public Color backgroundColor = new Color(0.4f, 0.4f, 0.4f); // Background color
    }

    /// Captures a tightly-framed screenshot of the given object from a specified direction.
    /// targetObject: The object to capture.
    /// settings: Screenshot and camera settings.
    /// incomingRayDirection: Ray direction to determine viewing angle.
    /// Base64-encoded JPEG screenshot string.
    public static string CaptureObjectScreenshot(
        LookableObject targetObject,
        CaptureSettings settings,
        Vector3 incomingRayDirection
    )
    {
        if (targetObject == null)
        {
            Debug.LogError("Target LookableObject not assigned.");
            return null;
        }

        Renderer renderer = targetObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Target LookableObject does not have a Renderer.");
            return null;
        }

        // Calculate object bounds and center
        Bounds bounds = renderer.bounds;
        Vector3 center = bounds.center;
        float boundingRadius = bounds.extents.magnitude;

        // Compute camera distance based on bounding size and FOV
        float fovRad = settings.fieldOfView * Mathf.Deg2Rad;
        float distance = boundingRadius / Mathf.Tan(fovRad / 2f);
        distance *= 1.1f; // Slight padding

        // Direction from ray
        Vector3 cameraDirection = -incomingRayDirection.normalized; // - is for taking photo in front of the object
        Vector3 cameraPosition = center + cameraDirection * distance;  

        // Create temporary camera
        GameObject camObj = new GameObject("TempCaptureCamera");
        Camera tempCam = camObj.AddComponent<Camera>();
        tempCam.enabled = false;
        tempCam.clearFlags = CameraClearFlags.Color;
        tempCam.backgroundColor = settings.backgroundColor;
        tempCam.fieldOfView = settings.fieldOfView;

        // Assign only the target to a dedicated layer
        int originalLayer = targetObject.gameObject.layer;
        int screenshotLayer = LayerMask.NameToLayer("ScreenshotOnly");
        if (screenshotLayer == -1)
        {
            Debug.LogError("Layer 'ScreenshotOnly' does not exist. Please define it in Unity Tags and Layers.");
            UnityEngine.Object.Destroy(camObj);
            return null;
        }

        ApplyLayerRecursively(targetObject.gameObject, screenshotLayer);
        tempCam.cullingMask = 1 << screenshotLayer;

        // Set camera position and orientation
        camObj.transform.position = cameraPosition;
        camObj.transform.LookAt(center);

        // Setup render texture and capture
        RenderTexture rt = new RenderTexture(settings.width, settings.height, 24);
        Texture2D tex = new Texture2D(settings.width, settings.height, TextureFormat.RGB24, false);

        tempCam.targetTexture = rt;
        tempCam.Render();

        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, settings.width, settings.height), 0, 0);
        tex.Apply();

        tempCam.targetTexture = null;
        RenderTexture.active = null;

        // Cleanup
        UnityEngine.Object.Destroy(rt);
        UnityEngine.Object.Destroy(camObj);
        ApplyLayerRecursively(targetObject.gameObject, originalLayer);

        byte[] jpgBytes = tex.EncodeToJPG(settings.resolution);

        rt.Release();
        UnityEngine.Object.Destroy(rt);
        UnityEngine.Object.Destroy(camObj);
        ApplyLayerRecursively(targetObject.gameObject, originalLayer);
        UnityEngine.Object.Destroy(tex);

        // Save to disk for debug purposes
        string fileName = $"screenshot_{targetObject.name}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, jpgBytes);
        Debug.Log($"Screenshot saved for {targetObject.name} at {filePath}");
        
        return Convert.ToBase64String(jpgBytes);
    }

    // Recursively sets the layer of an object and all its children
    private static void ApplyLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            ApplyLayerRecursively(child.gameObject, layer);
        }
    }
}
