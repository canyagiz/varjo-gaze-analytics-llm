using System;
using System.IO;
using UnityEngine;

public static class SessionManager
{
    private static string counterPath => Path.Combine(Application.persistentDataPath, "session_counter.txt");
    private static int currentSessionId = -1;

    public static int GetSessionId()
    {
        if (currentSessionId == -1)
        {
            LoadOrGenerateSessionId();
        }
        return currentSessionId;
    }

    public static void GenerateNewSessionIfNeeded()
    {
        if (Application.isEditor && !Application.isPlaying)
            return; // Prevent accidental overwrite outside play mode

        LoadOrGenerateSessionId(true);
        Debug.Log($"[SessionManager] New session started: {currentSessionId}");
    }

    private static void LoadOrGenerateSessionId(bool forceNew = false)
    {
        if (!File.Exists(counterPath))
        {
            currentSessionId = 1;
        }
        else
        {
            string text = File.ReadAllText(counterPath);
            if (!int.TryParse(text, out currentSessionId))
                currentSessionId = 1;
        }

        if (forceNew)
        {
            currentSessionId++;
            File.WriteAllText(counterPath, currentSessionId.ToString());
        }
    }
}
