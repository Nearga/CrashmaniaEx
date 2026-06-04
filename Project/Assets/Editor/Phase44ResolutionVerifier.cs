#if UNITY_EDITOR
using System;
using System.IO;
using Crashmania.UI.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase44ResolutionVerifier
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/Login.unity",
            "Assets/Scenes/Lobby.unity",
            "Assets/Scenes/Store.unity",
            "Assets/Scenes/Gifts.unity",
            "Assets/Scenes/Account.unity",
            "Assets/Scenes/Game.unity"
        };

        private static readonly string[] SafeAreaScenePaths =
        {
            "Assets/Scenes/Lobby.unity",
            "Assets/Scenes/Game.unity"
        };

        [MenuItem("Crashmania/Verify Phase 4.4 Resolution")]
        public static void Run()
        {
            VerifySceneCanvasScalers();
            VerifySceneSafeAreas();
            VerifyPortraitOnly();
            VerifyIPhoneQuality();
            Debug.Log("[Phase44ResolutionVerifier] Phase 4.4 resolution verification completed.");
        }

        private static void VerifySceneCanvasScalers()
        {
            foreach (var scenePath in ScenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);
                if (!scene.IsValid())
                {
                    throw new InvalidOperationException($"Could not open scene: {scenePath}");
                }

                var canvases = GetSceneComponents<Canvas>(scene);
                if (canvases.Count != 1)
                {
                    throw new InvalidOperationException($"{scenePath} must contain exactly one scene Canvas. Found: {canvases.Count}");
                }

                var scaler = canvases[0].GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    throw new InvalidOperationException($"{scenePath} root Canvas has no CanvasScaler.");
                }

                AssertCanvasScalerPolicy(scaler, scenePath);
            }
        }

        private static void VerifySceneSafeAreas()
        {
            foreach (var scenePath in SafeAreaScenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);
                if (GetSceneComponents<SafeAreaPanel>(scene).Count == 0)
                {
                    throw new InvalidOperationException($"{scenePath} is missing a scene-level SafeAreaPanel for interactive chrome.");
                }
            }
        }

        private static System.Collections.Generic.List<T> GetSceneComponents<T>(UnityEngine.SceneManagement.Scene scene)
            where T : Component
        {
            var components = new System.Collections.Generic.List<T>();
            foreach (var root in scene.GetRootGameObjects())
            {
                components.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return components;
        }

        private static void VerifyPortraitOnly()
        {
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait ||
                PlayerSettings.allowedAutorotateToLandscapeLeft ||
                PlayerSettings.allowedAutorotateToLandscapeRight ||
                PlayerSettings.allowedAutorotateToPortraitUpsideDown)
            {
                throw new InvalidOperationException("iOS orientation settings must be portrait-only.");
            }
        }

        private static void VerifyIPhoneQuality()
        {
            var pipelineAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (pipelineAsset == null)
            {
                throw new InvalidOperationException("Default render pipeline is not URP.");
            }

            if (Math.Abs(pipelineAsset.renderScale - 1f) > 0.001f)
            {
                throw new InvalidOperationException("URP render scale must remain 1.0 for iPhone UI fidelity.");
            }

            var qualitySettingsPath = Path.Combine(Application.dataPath, "../ProjectSettings/QualitySettings.asset");
            var qualitySettings = File.ReadAllText(qualitySettingsPath);
            if (!qualitySettings.Contains("iPhone: 5"))
            {
                throw new InvalidOperationException("iPhone default quality must use the URP-backed quality tier.");
            }
        }

        private static void AssertCanvasScalerPolicy(CanvasScaler scaler, string owner)
        {
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                scaler.referenceResolution != CanvasResolutionPolicy.ReferenceResolution ||
                scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ||
                Math.Abs(scaler.matchWidthOrHeight - CanvasResolutionPolicy.MatchWidthOrHeight) > 0.001f)
            {
                throw new InvalidOperationException($"{owner} CanvasScaler does not match the iPhone portrait resolution policy.");
            }
        }
    }
}
#endif
