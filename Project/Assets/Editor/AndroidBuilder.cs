#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

namespace Crashmania.Editor
{
    public static class AndroidBuilder
    {
        [MenuItem("Crashmania/Build Android Project - Debug (Fast Iteration)")]
        public static void BuildAndroidDebug()
        {
            PerformBuild(isDebug: true);
        }

        [MenuItem("Crashmania/Build Android Project - Release (Full)")]
        public static void BuildAndroidRelease()
        {
            PerformBuild(isDebug: false);
        }

        private static void PerformBuild(bool isDebug)
        {
            string buildType = isDebug ? "DEBUG" : "RELEASE";
            Debug.Log($"[AndroidBuilder] Starting Android {buildType} build process...");

            // 1. Switch Target
            try
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AndroidBuilder] Could not switch build target: {ex.Message}. Make sure Android module is installed.");
                return;
            }

            // 2. Configure Player Settings
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            
            if (isDebug)
            {
                Debug.Log("[AndroidBuilder] Optimizing for Debug: Architecture=ARM64, IL2CPP=Debug");
                SetArchitecture(2); // ARM64 Only
                PlayerSettings.SetIl2CppCompilerConfiguration(UnityEditor.Build.NamedBuildTarget.Android, Il2CppCompilerConfiguration.Debug);
            }
            else
            {
                Debug.Log("[AndroidBuilder] Using Release settings: Architecture=ARMv7+ARM64, IL2CPP=Release");
                SetArchitecture(3); // ARMv7 + ARM64
                PlayerSettings.SetIl2CppCompilerConfiguration(UnityEditor.Build.NamedBuildTarget.Android, Il2CppCompilerConfiguration.Release);
            }

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
            
            PlayerSettings.companyName = "Crashmania";
            PlayerSettings.productName = "CrashmaniaEx";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.crashmania.casino");

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            
            // Minimal stripping is safer for local iteration with our link.xml protection
            PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);

            // 3. Define scenes to include
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[AndroidBuilder] No enabled scenes found in Editor Build Settings!");
                return;
            }

            // 4. Configure Output Path
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string buildFolder = Path.Combine(projectRoot, "Builds", "Android");
            string apkName = isDebug ? "Crashmania_Debug.apk" : "Crashmania_Release.apk";
            string buildPath = Path.Combine(buildFolder, apkName);
            Directory.CreateDirectory(buildFolder);

            // 5. Configure Build Options
            BuildOptions options = isDebug 
                ? BuildOptions.Development | BuildOptions.AllowDebugging 
                : BuildOptions.None;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = options
            };

            // 6. Run the build
            Debug.Log($"[AndroidBuilder] Building {buildType} APK to: {buildPath}");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[AndroidBuilder] {buildType} Build SUCCEEDED: {summary.totalSize} bytes exported to {buildPath}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[AndroidBuilder] {buildType} Build FAILED! Check the console for details.");
            }
        }

        private static void SetArchitecture(int architectureValue)
        {
            // ARMv7 (1) | ARM64 (2)
            var methods = typeof(PlayerSettings).GetMethods()
                .Where(method => method.Name == "SetArchitecture" && method.GetParameters().Length == 2);

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                try
                {
                    if (parameters[0].ParameterType == typeof(UnityEditor.Build.NamedBuildTarget))
                    {
                        method.Invoke(null, new object[] { UnityEditor.Build.NamedBuildTarget.Android, architectureValue });
                        return;
                    }

                    if (parameters[0].ParameterType == typeof(BuildTargetGroup))
                    {
                        method.Invoke(null, new object[] { BuildTargetGroup.Android, architectureValue });
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AndroidBuilder] Could not set architecture: {ex.Message}");
                }
            }
        }
    }
}
#endif
