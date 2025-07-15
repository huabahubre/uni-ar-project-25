using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    public string folderPath = "Assets/Screenshots";
    public KeyCode screenshotKey = KeyCode.F12;

    private void Start()
    {
        Debug.Log($"Screen: {Screen.width}x{Screen.height}");
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(screenshotKey))
        {
            TakeScreenshot();
        }
#endif
    }

    [Button]
    public void TakeScreenshot()
    {
        // Prevent from running in clone instances or headless
        if (!Application.isFocused || Application.isBatchMode)
        {
            Debug.LogWarning("Screenshot skipped: Not in focused window.");
            return;
        }

        string path = $"Assets/Screenshots/Screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log($"✅ Screenshot saved: {path}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}