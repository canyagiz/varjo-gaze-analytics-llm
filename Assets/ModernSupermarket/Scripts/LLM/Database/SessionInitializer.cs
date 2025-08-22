#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SessionInitializer
{
    static SessionInitializer()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            SessionManager.GenerateNewSessionIfNeeded();
            Debug.Log("[SessionInitializer] New session started.");
        }
    }
}
#endif
