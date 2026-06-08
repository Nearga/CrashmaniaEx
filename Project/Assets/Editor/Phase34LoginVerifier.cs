#if UNITY_EDITOR
using System;
using System.IO;
using Crashmania.PureMvc.Scenes;
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
        private static readonly Vector2 AndroidReferenceResolution = new(1080f, 1920f);
        private const string HeroSpritePath = "Assets/Resources/UI/Textures/Login/homepage-banner-mobile.png";

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

        private static readonly string[] RequiredSectionPaths =
        {
            "LoginCanvas/ScrollRect/Viewport/Content/HeroSection",
            "LoginCanvas/ScrollRect/Viewport/Content/HeroDivider",
            "LoginCanvas/ScrollRect/Viewport/Content/GameGallerySection",
            "LoginCanvas/ScrollRect/Viewport/Content/DarkDivider",
            "LoginCanvas/ScrollRect/Viewport/Content/LegalFooterSection"
        };

        private static readonly string[] RequiredSpritePaths =
        {
            HeroSpritePath,
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
            VerifyAndroidVisualStructure();
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
                scaler.referenceResolution != AndroidReferenceResolution ||
                scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.Expand ||
                Math.Abs(scaler.referencePixelsPerUnit - 100f) > 0.001f)
            {
                throw new InvalidOperationException("LoginCanvas CanvasScaler does not match the Android layout policy.");
            }

            var canvasComponent = canvas.GetComponent<Canvas>();
            if (canvasComponent == null || canvasComponent.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                throw new InvalidOperationException("LoginCanvas must render as Screen Space - Overlay for Android login layout.");
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

        private static void VerifyAndroidVisualStructure()
        {
            var content = GameObject.Find("LoginCanvas/ScrollRect/Viewport/Content")?.GetComponent<RectTransform>();
            if (content == null)
            {
                throw new InvalidOperationException("Login Content RectTransform is missing.");
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            Canvas.ForceUpdateCanvases();

            var heroImage = GameObject.Find("LoginCanvas/ScrollRect/Viewport/Content/HeroSection")?.GetComponent<Image>();
            if (heroImage == null || heroImage.sprite == null ||
                AssetDatabase.GetAssetPath(heroImage.sprite) != HeroSpritePath)
            {
                throw new InvalidOperationException("HeroSection must use the homepage-banner-mobile source art.");
            }

            foreach (var path in RequiredSectionPaths)
            {
                var section = GameObject.Find(path);
                var layout = section != null ? section.GetComponent<LayoutElement>() : null;
                var rect = section != null ? section.GetComponent<RectTransform>() : null;
                if (layout == null || layout.preferredHeight <= 0f)
                {
                    throw new InvalidOperationException($"{path} must have a positive LayoutElement preferred height.");
                }

                if ((path.EndsWith("HeroSection", StringComparison.Ordinal) ||
                     path.EndsWith("GameGallerySection", StringComparison.Ordinal) ||
                     path.EndsWith("LegalFooterSection", StringComparison.Ordinal)) &&
                    (rect == null || rect.rect.width <= 1f || rect.rect.height <= 1f))
                {
                    throw new InvalidOperationException($"{path} must have a nonzero resolved layout rect.");
                }
            }

            var toastPanel = GameObject.Find("LoginCanvas/ToastOverlay/Safe Area/Toast Panel")?.GetComponent<RectTransform>();
            if (toastPanel == null)
            {
                throw new InvalidOperationException("Login Toast Panel is missing.");
            }

            if (toastPanel.rect.height > 160f || toastPanel.sizeDelta.y > 160f)
            {
                throw new InvalidOperationException("Login Toast Panel must be a compact toast, not a full-screen overlay.");
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
