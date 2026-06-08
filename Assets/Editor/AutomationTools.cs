using System.IO;
using UnityEngine;
using UnityEditor;

public static class AutomationTools {
    [MenuItem("Automation/Snapshot")]
    public static void TakeScreenshot() {
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "Builds/Automation");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        string file = Path.Combine(dir, "current_frame.png");
        ScreenCapture.CaptureScreenshot(file);
        AssetDatabase.Refresh();
        Debug.Log("[Harness] Snapshot saved to: " + file);
    }
}