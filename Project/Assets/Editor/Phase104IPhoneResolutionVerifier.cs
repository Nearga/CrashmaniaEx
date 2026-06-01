#if UNITY_EDITOR
using System;
using System.IO;
using Crashmania.Game;
using Crashmania.UI.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase104IPhoneResolutionVerifier
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

        [MenuItem("Crashmania/Verify Phase 10.4 iPhone Resolution Strategy")]
        public static void Run()
        {
            VerifyPolicyConstants();
            VerifyRuntimeOrientationPolicySource();
            VerifyStartupAppliesResolutionPolicy();
            VerifySceneOrientationPolicies();
            VerifySafeAreaPanelRefreshesAtRuntime();
            VerifySceneCanvases();
            VerifyPrefabCanvases();
            VerifyGameDynamicLayout();
            VerifyAutorotationSettings();
            Debug.Log("[Phase104IPhoneResolutionVerifier] Phase 10.4 iPhone resolution strategy verification completed.");
        }

        private static void VerifyPolicyConstants()
        {
            if (CanvasResolutionPolicy.ReferenceResolution != new Vector2(1170f, 2532f) ||
                Math.Abs(CanvasResolutionPolicy.MatchWidthOrHeight) > 0.001f ||
                CanvasResolutionPolicy.TargetFrameRate != 60)
            {
                throw new InvalidOperationException("CanvasResolutionPolicy must stay portrait-first at 1170x2532, width-match 0.0, 60 FPS.");
            }

            if (MobileResolutionPolicy.MaxCrashGameRenderHeight != 1440 ||
                MobileResolutionPolicy.MaxHeavy3DRenderHeight != 1080 ||
                MobileResolutionPolicy.MaxLongScreenWidth != 2340 ||
                MobileResolutionPolicy.MinRecommendedTargetDpi != 200 ||
                MobileResolutionPolicy.MaxRecommendedTargetDpi != 300)
            {
                throw new InvalidOperationException("MobileResolutionPolicy constants must match the approved iPhone crash-game strategy.");
            }

            var clamped = MobileResolutionPolicy.CalculateClampedResolution(1290, 2796, MobileResolutionPolicy.MaxCrashGameRenderHeight);
            if (clamped.y != 1440 || clamped.x <= 0 || clamped.x >= 1290)
            {
                throw new InvalidOperationException("MobileResolutionPolicy must clamp high-end Retina portrait screens to a 1440 long side while preserving aspect ratio.");
            }
        }

        private static void VerifyRuntimeOrientationPolicySource()
        {
            var sourcePath = Path.Combine(Application.dataPath, "Scripts/UI/Components/SceneOrientationPolicy.cs");
            var source = File.ReadAllText(sourcePath);
            RequireContains(source, "LockPortrait", sourcePath);
            RequireContains(source, "AllowAutoRotation", sourcePath);
            RequireContains(source, "ScreenOrientation.Portrait", sourcePath);
            RequireContains(source, "ScreenOrientation.AutoRotation", sourcePath);
            RequireContains(source, "autorotateToPortraitUpsideDown = false", sourcePath);
            RequireContains(source, "autorotateToLandscapeLeft = true", sourcePath);
            RequireContains(source, "autorotateToLandscapeRight = true", sourcePath);
        }

        private static void VerifyStartupAppliesResolutionPolicy()
        {
            var startupPath = Path.Combine(Application.dataPath, "Scripts/Core/Startup.cs");
            var startup = File.ReadAllText(startupPath);
            RequireContains(startup, "MobileResolutionPolicy.ApplyRuntimePolicy()", startupPath);
        }

        private static void VerifySceneOrientationPolicies()
        {
            var portraitScenes = new[]
            {
                "Assets/Scenes/Boot.unity",
                "Assets/Scenes/Login.unity",
                "Assets/Scenes/Lobby.unity",
                "Assets/Scenes/Store.unity",
                "Assets/Scenes/Gifts.unity",
                "Assets/Scenes/Account.unity"
            };

            foreach (var scenePath in portraitScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);
                if (!scene.IsValid())
                {
                    throw new InvalidOperationException($"Could not open scene: {scenePath}");
                }

                var policy = UnityEngine.Object.FindAnyObjectByType<SceneOrientationPolicy>();
                if (policy == null)
                {
                    throw new InvalidOperationException($"{scenePath} must have a SceneOrientationPolicy component on its Canvas.");
                }

                if (policy.GetComponent<Canvas>() == null)
                {
                    throw new InvalidOperationException($"{scenePath} SceneOrientationPolicy must be on a Canvas GameObject, not '{policy.gameObject.name}'.");
                }

                if (policy.Mode != OrientationMode.ForcePortrait)
                {
                    throw new InvalidOperationException($"{scenePath} SceneOrientationPolicy must be ForcePortrait.");
                }
            }

            var gameScene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
            if (!gameScene.IsValid())
            {
                throw new InvalidOperationException("Could not open Game scene.");
            }

            var gamePolicy = UnityEngine.Object.FindAnyObjectByType<SceneOrientationPolicy>();
            if (gamePolicy == null)
            {
                throw new InvalidOperationException("Game scene must have a SceneOrientationPolicy component on GameCanvas.");
            }

            if (gamePolicy.GetComponent<Canvas>() == null)
            {
                throw new InvalidOperationException($"Game scene SceneOrientationPolicy must be on GameCanvas, not '{gamePolicy.gameObject.name}'.");
            }

            if (gamePolicy.Mode != OrientationMode.PortraitOrLandscape)
            {
                throw new InvalidOperationException("Game scene SceneOrientationPolicy must be PortraitOrLandscape.");
            }
        }

        private static void VerifySafeAreaPanelRefreshesAtRuntime()
        {
            var sourcePath = Path.Combine(Application.dataPath, "Scripts/UI/Components/SafeAreaPanel.cs");
            var source = File.ReadAllText(sourcePath);
            if (!source.Contains("private void Update()", StringComparison.Ordinal) ||
                !source.Contains("Screen.safeArea", StringComparison.Ordinal) ||
                !source.Contains("lastScreenSize", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("SafeAreaPanel must monitor Screen.safeArea and screen-size changes at runtime.");
            }
        }

        private static void VerifySceneCanvases()
        {
            foreach (var scenePath in ScenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);
                if (!scene.IsValid())
                {
                    throw new InvalidOperationException($"Could not open scene: {scenePath}");
                }

                var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
                if (canvases.Length == 0)
                {
                    throw new InvalidOperationException($"{scenePath} has no Canvas.");
                }

                foreach (var canvas in canvases)
                {
                    AssertCanvas(canvas, scenePath);
                }

                var legacyTexts = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include);
                if (legacyTexts.Length > 0)
                {
                    throw new InvalidOperationException($"{scenePath} contains legacy Unity UI Text; use TextMeshPro for high-DPI iPhone text.");
                }
            }
        }

        private static void VerifyPrefabCanvases()
        {
            foreach (var prefabPath in CanvasPrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    throw new InvalidOperationException($"Missing UI prefab: {prefabPath}");
                }

                foreach (var canvas in prefab.GetComponentsInChildren<Canvas>(true))
                {
                    AssertCanvas(canvas, prefabPath);
                }

                if (prefab.GetComponentsInChildren<Text>(true).Length > 0)
                {
                    throw new InvalidOperationException($"{prefabPath} contains legacy Unity UI Text; use TextMeshPro for high-DPI iPhone text.");
                }
            }
        }

        private static void VerifyGameDynamicLayout()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
            if (!scene.IsValid())
            {
                throw new InvalidOperationException("Could not open Game scene.");
            }

            var gameCanvas = GameObject.Find("GameCanvas");
            if (gameCanvas == null)
            {
                throw new InvalidOperationException("Game scene must contain GameCanvas.");
            }

            var manager = gameCanvas.GetComponent<DynamicOrientationManager>();
            if (manager == null)
            {
                throw new InvalidOperationException("GameCanvas must own DynamicOrientationManager.");
            }

            var safeAreaWrapper = FindChild(gameCanvas.transform, "SafeAreaWrapper");
            var decorativeRoot = FindChild(gameCanvas.transform, "DecorativeBackgroundRoot");
            var portraitRoot = FindChild(gameCanvas.transform, "Portrait_LayoutRoot");
            var landscapeRoot = FindChild(gameCanvas.transform, "Landscape_LayoutRoot");

            if (safeAreaWrapper == null || decorativeRoot == null || portraitRoot == null || landscapeRoot == null)
            {
                throw new InvalidOperationException("GameCanvas must contain DecorativeBackgroundRoot, SafeAreaWrapper, Portrait_LayoutRoot, and Landscape_LayoutRoot.");
            }

            if (safeAreaWrapper.GetComponent<SafeAreaPanel>() == null)
            {
                throw new InvalidOperationException("SafeAreaWrapper must carry SafeAreaPanel for interactive Game UI.");
            }

            if (decorativeRoot.transform.IsChildOf(safeAreaWrapper.transform))
            {
                throw new InvalidOperationException("DecorativeBackgroundRoot must not be constrained by SafeAreaWrapper.");
            }

            var decorativeChildren = new[] { "FlightSpaceBackground", "Stars", "Planet", "Asteroids", "GroundOrMoonLayer", "SpeedLines", "GridBackground", "CrashTint", "CountdownBackground" };
            foreach (var decorativeName in decorativeChildren)
            {
                var decorativeChild = FindChild(decorativeRoot.transform, decorativeName);
                if (decorativeChild == null)
                {
                    throw new InvalidOperationException($"DecorativeBackgroundRoot must contain {decorativeName}. It should not be inside SafeAreaWrapper or GameViewportContainer.");
                }

                if (decorativeChild.transform.IsChildOf(safeAreaWrapper.transform))
                {
                    throw new InvalidOperationException($"{decorativeName} must be under DecorativeBackgroundRoot, not inside SafeAreaWrapper.");
                }
            }

            if (portraitRoot.GetComponent<CrashGameLayoutView>() == null || landscapeRoot.GetComponent<CrashGameLayoutView>() == null)
            {
                throw new InvalidOperationException("Both Game orientation roots must carry CrashGameLayoutView.");
            }

            var canvasScaler = gameCanvas.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                throw new InvalidOperationException("GameCanvas must carry CanvasScaler.");
            }

            manager.ForcePortraitForVerifier();
            if (!portraitRoot.activeSelf || landscapeRoot.activeSelf || Math.Abs(canvasScaler.matchWidthOrHeight - 0f) > 0.001f)
            {
                throw new InvalidOperationException("DynamicOrientationManager must activate portrait root with width-match 0.0.");
            }

            manager.ForceLandscapeForVerifier();
            if (portraitRoot.activeSelf || !landscapeRoot.activeSelf || Math.Abs(canvasScaler.matchWidthOrHeight - 1f) > 0.001f)
            {
                throw new InvalidOperationException("DynamicOrientationManager must activate landscape root with height-match 1.0.");
            }

            manager.ForcePortraitForVerifier();
        }

        private static void VerifyAutorotationSettings()
        {
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.AutoRotation ||
                !PlayerSettings.allowedAutorotateToPortrait ||
                PlayerSettings.allowedAutorotateToPortraitUpsideDown ||
                !PlayerSettings.allowedAutorotateToLandscapeLeft ||
                !PlayerSettings.allowedAutorotateToLandscapeRight)
            {
                throw new InvalidOperationException("iOS Player Settings must allow portrait and landscape for Game, with upside-down disabled; runtime policy locks all non-game scenes to portrait.");
            }
        }

        private static void AssertCanvas(Canvas canvas, string owner)
        {
            if (canvas.pixelPerfect)
            {
                throw new InvalidOperationException($"{owner} canvas '{canvas.name}' must have Pixel Perfect disabled for high-frequency Retina UI updates.");
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                throw new InvalidOperationException($"{owner} canvas '{canvas.name}' is missing CanvasScaler.");
            }

            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                scaler.referenceResolution != CanvasResolutionPolicy.ReferenceResolution ||
                scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ||
                Math.Abs(scaler.matchWidthOrHeight - CanvasResolutionPolicy.MatchWidthOrHeight) > 0.001f)
            {
                throw new InvalidOperationException($"{owner} canvas '{canvas.name}' does not match the portrait iPhone CanvasScaler policy in saved scene/prefab state.");
            }
        }

        private static GameObject FindChild(Transform root, string childName)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static void RequireContains(string source, string needle, string owner)
        {
            if (!source.Contains(needle, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{owner} must contain '{needle}'.");
            }
        }
    }
}
#endif
