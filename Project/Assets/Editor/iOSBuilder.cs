#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Crashmania.Editor
{
    public static class iOSBuilder
    {
        [MenuItem("Crashmania/Build iOS Project")]
        public static void BuildiOS()
        {
            Debug.Log("[iOSBuilder] Initiating iOS project build...");


            // 1. Switch to iOS target platform
            try
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                Debug.Log("[iOSBuilder] Switched active build target to iOS.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[iOSBuilder] Failed to switch build target: {ex.Message}");
                return;
            }

            // 2. Configure Player Settings for iOS
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.iOS.targetOSVersionString = "16.0";

            // Use Minimal stripping to avoid UnityLinker failures with reflection-heavy
            // packages (MCPForUnity, Unity.AI.*, glTFast, etc.) that are included in the build.
            // This is safe for a development/sideload build; tighten later if App Store size matters.
            PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.iOS, ManagedStrippingLevel.Minimal);

            // 3. Define scenes to include in build from EditorBuildSettings
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[iOSBuilder] No enabled scenes found in Editor Build Settings!");
                return;
            }

            Debug.Log($"[iOSBuilder] Including scenes:\n{string.Join("\n", scenes)}");

            // Absolute path: Project/Builds/iOS (derived from Assets/ folder location)
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string buildPath = Path.Combine(projectRoot, "Builds", "iOS");
            Directory.CreateDirectory(buildPath);
            Debug.Log($"[iOSBuilder] Build output path: {buildPath}");

            // 4. Configure Build options
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            // 5. Run the build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[iOSBuilder] Build SUCCEEDED: {summary.totalSize} bytes exported to {buildPath}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("[iOSBuilder] Build FAILED! Check the console/editor logs for details.");
            }
        }
    }
}
#endif
