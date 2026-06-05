#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AutomationTools
{
    [MenuItem("Automation/Take Screenshot")]
    public static void TakeScreenshot()
    {
        string directory = Path.Combine(Directory.GetCurrentDirectory(), "Builds/Automation");
        Directory.CreateDirectory(directory);

        string filePath = Path.Combine(directory, "current_frame.png");
        ScreenCapture.CaptureScreenshot(filePath);

        AssetDatabase.Refresh();
        Debug.Log($"[Automation] Screenshot captured to: {filePath}");
    }
}
#endif
