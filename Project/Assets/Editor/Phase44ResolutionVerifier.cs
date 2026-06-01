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

        private static readonly string[] CanvasPrefabPaths =
        {
            "Assets/Resources/UI/Prefabs/HeaderOverlay.prefab",
            "Assets/Resources/UI/Prefabs/TabBarOverlay.prefab",
            "Assets/Resources/UI/Prefabs/ModalManagerOverlay.prefab",
            "Assets/Resources/UI/Prefabs/ToastOverlay.prefab",
            "Assets/Resources/UI/Prefabs/LoginScreen.prefab"
        };

        private static readonly string[] SafeAreaPrefabPaths =
        {
            "Assets/Resources/UI/Prefabs/HeaderOverlay.prefab",
            "Assets/Resources/UI/Prefabs/TabBarOverlay.prefab",
            "Assets/Resources/UI/Prefabs/ToastOverlay.prefab"
        };

        [MenuItem("Crashmania/Verify Phase 4.4 Resolution")]
        public static void Run()
        {
            VerifySceneCanvasScalers();
            VerifyPrefabCanvasScalers();
            VerifySafeAreaChrome();
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

                var scalers = UnityEngine.Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (scalers.Length == 0)
                {
                    throw new InvalidOperationException($"{scenePath} has no CanvasScaler.");
                }

                foreach (var scaler in scalers)
                {
                    AssertCanvasScalerPolicy(scaler, scenePath);
                }
            }
        }

        private static void VerifyPrefabCanvasScalers()
        {
            foreach (var prefabPath in CanvasPrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    throw new InvalidOperationException($"Missing UI prefab: {prefabPath}");
                }

                var scalers = prefab.GetComponentsInChildren<CanvasScaler>(true);
                if (scalers.Length == 0)
                {
                    throw new InvalidOperationException($"{prefabPath} has no CanvasScaler.");
                }

                foreach (var scaler in scalers)
                {
                    AssertCanvasScalerPolicy(scaler, prefabPath);
                }
            }
        }

        private static void VerifySafeAreaChrome()
        {
            foreach (var prefabPath in SafeAreaPrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null || prefab.GetComponentInChildren<SafeAreaPanel>(true) == null)
                {
                    throw new InvalidOperationException($"{prefabPath} is missing SafeAreaPanel on interactive chrome.");
                }
            }
        }

private static void VerifyPortraitOnly()
        {
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.AutoRotation ||
                !PlayerSettings.allowedAutorotateToPortrait ||
                PlayerSettings.allowedAutorotateToPortraitUpsideDown ||
                !PlayerSettings.allowedAutorotateToLandscapeLeft ||
                !PlayerSettings.allowedAutorotateToLandscapeRight)
            {
                throw new InvalidOperationException("iOS orientation settings must allow Game landscape while runtime policy locks all non-game scenes to portrait.");
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
