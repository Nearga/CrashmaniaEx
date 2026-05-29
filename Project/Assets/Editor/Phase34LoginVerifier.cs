#if UNITY_EDITOR
using System;
using System.IO;
using Crashmania.PureMvc.Scenes;
using Crashmania.UI.Components;
using Crashmania.UI.Login;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase34LoginVerifier
    {
        private const string LoginScenePath = "Assets/Scenes/Login.unity";

        private static readonly string[] RequiredPaths =
        {
            "LoginCanvas",
            "LoginCanvas/ScrollRect",
            "LoginCanvas/ScrollRect/Viewport",
            "LoginCanvas/ScrollRect/Viewport/Content",
            "LoginCanvas/ScrollRect/Viewport/Content/HeroSection",
            "LoginCanvas/ScrollRect/Viewport/Content/HeroDivider",
            "LoginCanvas/ScrollRect/Viewport/Content/GameGallerySection",
            "LoginCanvas/ScrollRect/Viewport/Content/DarkDivider",
            "LoginCanvas/ScrollRect/Viewport/Content/LegalFooterSection",
            "LoginCanvas/ScrollRect/Viewport/Content/HeroSection/Header/LogInButton",
            "LoginCanvas/ScrollRect/Viewport/Content/HeroSection/Header/SignUpButton",
            "LoginCanvas/ScrollRect/Viewport/Content/HeroSection/Bonus/JoinNowButton",
            "LoginCanvas/ScrollRect/Viewport/Content/HeroSection/Bonus/NoPurchaseUnderJoinText",
            "LoginCanvas/ScrollRect/Viewport/Content/GameGallerySection/PlayForFreeButton"
        };

        private static readonly string[] RequiredSpritePaths =
        {
            "Assets/Resources/UI/Textures/Login/homepage-banner-mobile.png",
            "Assets/Resources/UI/Textures/Login/hompage-divider-mobile.png",
            "Assets/Resources/UI/Textures/Login/logo.png",
            "Assets/Resources/UI/Textures/Login/top-coin.png",
            "Assets/Resources/UI/Icons/Game/coin.png",
            "Assets/Resources/UI/Icons/Game/sweep-coin.png",
            "Assets/Resources/UI/Games/Homepage/astro_go.png",
            "Assets/Resources/UI/Games/Homepage/crush_depth.png",
            "Assets/Resources/UI/Games/Homepage/fightX.png",
            "Assets/Resources/UI/Games/Homepage/moon_juggling.png",
            "Assets/Resources/UI/Games/Homepage/rise_up.png",
            "Assets/Resources/UI/Games/Homepage/skyride.png",
            "Assets/Resources/UI/Games/Homepage/slackliner.png",
            "Assets/Resources/UI/Games/Homepage/swoosh_up.png",
            "Assets/Resources/UI/Games/Homepage/tiltx.png",
            "Assets/Resources/UI/Games/Homepage/bountiful-birds.png"
        };

        [MenuItem("Crashmania/Verify Phase 3.4 Login Scene")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene(LoginScenePath);
            if (!scene.IsValid())
            {
                throw new InvalidOperationException($"Could not open scene: {LoginScenePath}");
            }

            VerifyHierarchy();
            VerifyLoginView();
            VerifySceneController();
            VerifyCanvasScaler();
            VerifyTopDragAndHeader();
            VerifyAssets();
            VerifyNoPrefabFallback();
            Debug.Log("[Phase34LoginVerifier] Phase 3.4 login scene verification completed.");
        }

        private static void VerifyHierarchy()
        {
            foreach (var path in RequiredPaths)
            {
                if (GameObject.Find(path) == null)
                {
                    throw new InvalidOperationException($"{LoginScenePath} is missing required object: {path}");
                }
            }

            if (GameObject.Find("LoginScreen") != null)
            {
                throw new InvalidOperationException("Login.unity must not use the LoginScreen prefab instance as the runtime screen.");
            }
        }

        private static void VerifyLoginView()
        {
            var views = UnityEngine.Object.FindObjectsByType<LoginView>(FindObjectsInactive.Include);
            if (views.Length != 1)
            {
                throw new InvalidOperationException($"Login.unity must contain exactly one in-scene LoginView. Found: {views.Length}");
            }

            if (PrefabUtility.GetPrefabInstanceStatus(views[0].gameObject) != PrefabInstanceStatus.NotAPrefab)
            {
                throw new InvalidOperationException("LoginView must be part of the scene hierarchy, not a full-screen prefab instance.");
            }

            foreach (var path in RequiredPaths)
            {
                var go = GameObject.Find(path);
                if (go != null && path.EndsWith("Button", StringComparison.Ordinal) && go.GetComponent<Button>() == null)
                {
                    throw new InvalidOperationException($"{path} is missing a Button component.");
                }
            }
        }

        private static void VerifySceneController()
        {
            var controllers = UnityEngine.Object.FindObjectsByType<LoginSceneController>(FindObjectsInactive.Include);
            if (controllers.Length != 1)
            {
                throw new InvalidOperationException($"Login.unity must contain exactly one LoginSceneController. Found: {controllers.Length}");
            }
        }

        private static void VerifyCanvasScaler()
        {
            var canvas = GameObject.Find("LoginCanvas");
            var scaler = canvas != null ? canvas.GetComponent<CanvasScaler>() : null;
            if (scaler == null)
            {
                throw new InvalidOperationException("LoginCanvas is missing CanvasScaler.");
            }

            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                scaler.referenceResolution != CanvasResolutionPolicy.ReferenceResolution ||
                scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ||
                Math.Abs(scaler.matchWidthOrHeight - CanvasResolutionPolicy.MatchWidthOrHeight) > 0.001f)
            {
                throw new InvalidOperationException("LoginCanvas CanvasScaler does not match Phase 4.4 iPhone portrait policy.");
            }
        }

        private static void VerifyTopDragAndHeader()
        {
            var scrollRect = GameObject.Find("LoginCanvas/ScrollRect")?.GetComponent<ScrollRect>();
            if (scrollRect == null || scrollRect.movementType != ScrollRect.MovementType.Clamped)
            {
                throw new InvalidOperationException("Login ScrollRect must be clamped so dragging down does not reveal a top gap.");
            }

            var header = GameObject.Find("LoginCanvas/ScrollRect/Viewport/Content/HeroSection/Header");
            if (header == null)
            {
                throw new InvalidOperationException("Hero header is missing.");
            }

            if (header.GetComponent<Image>() != null || header.transform.Find("Background") != null)
            {
                throw new InvalidOperationException("Hero header must be transparent: logo/buttons overlay the hero art directly.");
            }
        }

        private static void VerifyAssets()
        {
            foreach (var path in RequiredSpritePaths)
            {
                if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null)
                {
                    throw new InvalidOperationException($"Missing required login sprite asset: {path}");
                }
            }
        }

        private static void VerifyNoPrefabFallback()
        {
            var sourcePath = Path.Combine(Application.dataPath, "Scripts/PureMvc/Commands/Navigation/SceneLoadedCommand.cs");
            var source = File.ReadAllText(sourcePath);
            if (source.Contains("UI/Prefabs/LoginScreen", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("SceneLoadedCommand must not instantiate the LoginScreen prefab fallback.");
            }

            if (source.Contains("LoginView", StringComparison.Ordinal) ||
                source.Contains("LoginMediator", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("SceneLoadedCommand must stay generic; Login-specific mediation belongs to LoginSceneController.");
            }
        }
    }
}
#endif
