#if UNITY_EDITOR
using System;
using System.IO;
using Crashmania.PureMvc.Commands.Lobby;
using Crashmania.PureMvc.Mediators;
using Crashmania.PureMvc.Proxies;
using Crashmania.PureMvc.Scenes;
using Crashmania.Services;
using Crashmania.UI.Components;
using Crashmania.UI.Lobby;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase5LobbyVerifier
    {
        private const string LobbyScenePath = "Assets/Scenes/Lobby.unity";

        private static readonly string[] RequiredScenePaths =
        {
            "LobbyCanvas",
            "LobbyCanvas/ContentSafeArea",
            "LobbyCanvas/HeaderOverlay/Safe Area/Header Bar",
            "LobbyCanvas/ContentSafeArea/ScrollRect",
            "LobbyCanvas/ContentSafeArea/ScrollRect/Viewport",
            "LobbyCanvas/ContentSafeArea/ScrollRect/Viewport/Content",
            "LobbyCanvas/ContentSafeArea/ScrollRect/Viewport/Content/PromoSection",
            "LobbyCanvas/ContentSafeArea/ScrollRect/Viewport/Content/RecentMultipliers",
            "LobbyCanvas/ContentSafeArea/ScrollRect/Viewport/Content/CategoryRail",
            "LobbyCanvas/ContentSafeArea/ScrollRect/Viewport/Content/CarouselSections"
        };

        private static readonly string[] RequiredPrefabPaths =
        {
            "Assets/Resources/UI/Prefabs/PromoBanner.prefab",
            "Assets/Resources/UI/Prefabs/CategoryChip.prefab",
            "Assets/Resources/UI/Prefabs/GameCard.prefab",
            "Assets/Resources/UI/Prefabs/GameCardTop10.prefab",
            "Assets/Resources/UI/Prefabs/GamesCarousel.prefab",
            "Assets/Resources/UI/Prefabs/HeaderOverlay.prefab",
            "Assets/Resources/UI/Prefabs/TabBarOverlay.prefab"
        };

        private static readonly string[] RequiredSpritePaths =
        {
            "Assets/Resources/UI/Promotions/Lobby/mission.png",
            "Assets/Resources/UI/Promotions/Lobby/lobby-bg.png",
            "Assets/Resources/UI/Promotions/Lobby/front-image.png",
            "Assets/Resources/UI/Promotions/Lobby/gift.png",
            "Assets/Resources/UI/Promotions/Lobby/gift-sweep.png",
            "Assets/Resources/UI/Games/Top10/1.png",
            "Assets/Resources/UI/Games/Homepage/astro_go.png",
            "Assets/Resources/UI/Games/Homepage/tiltx.png",
            "Assets/Resources/UI/NativeSprites/MGSlots-Lucky_Twins_Wilds_Jackpots.asset",
            "Assets/Resources/UI/NativeSprites/MGSlots-Bountiful_Birds.asset",
            "Assets/Resources/UI/NativeSprites/Crash-astro_go_thumbnail.asset",
            "Assets/Resources/UI/NativeSprites/Bottom Nav-home_icon_big.asset",
            "Assets/Resources/UI/NativeSprites/Top Bar-coin_crash.asset",
            "Assets/Resources/UI/NativeSprites/Body-Search.asset"
        };

        [MenuItem("Crashmania/Verify Phase 5 Lobby")]
        public static void Run()
        {
            VerifyTypes();
            VerifyAssets();
            VerifyMockData();
            VerifyScene();
            VerifyPureMvcBoundaries();
            Debug.Log("[Phase5LobbyVerifier] Phase 5 lobby verification completed.");
        }

        private static void VerifyTypes()
        {
            AssertType<CatalogProxy>();
            AssertType<LoadLobbyDataCommand>();
            AssertType<LobbyMediator>();
            AssertType<LobbySceneController>();
            AssertType<LobbyView>();
        }

        private static void VerifyAssets()
        {
            foreach (var prefabPath in RequiredPrefabPaths)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
                {
                    throw new InvalidOperationException($"Missing required lobby prefab: {prefabPath}");
                }
            }

            foreach (var spritePath in RequiredSpritePaths)
            {
                if (AssetDatabase.LoadAssetAtPath<Sprite>(spritePath) == null)
                {
                    throw new InvalidOperationException($"Missing required lobby sprite: {spritePath}");
                }
            }

            if (File.Exists(Path.Combine(Application.dataPath, "Editor/Phase5LobbyBuilder.cs")))
            {
                throw new InvalidOperationException("Phase 5 lobby must not depend on Phase5LobbyBuilder.cs.");
            }

            if (!File.Exists(Path.Combine(Application.dataPath, "../docs/phase5_asset_inventory.md")))
            {
                throw new InvalidOperationException("Missing Phase 5 asset inventory note.");
            }
        }

        private static void VerifyScene()
        {
            var scene = EditorSceneManager.OpenScene(LobbyScenePath);
            if (!scene.IsValid())
            {
                throw new InvalidOperationException($"Could not open scene: {LobbyScenePath}");
            }

            foreach (var path in RequiredScenePaths)
            {
                if (GameObject.Find(path) == null)
                {
                    throw new InvalidOperationException($"{LobbyScenePath} is missing required object: {path}");
                }
            }

            var views = UnityEngine.Object.FindObjectsByType<LobbyView>(FindObjectsInactive.Include);
            if (views.Length != 1)
            {
                throw new InvalidOperationException($"Lobby.unity must contain exactly one in-scene LobbyView. Found: {views.Length}");
            }

            var controllers = UnityEngine.Object.FindObjectsByType<LobbySceneController>(FindObjectsInactive.Include);
            if (controllers.Length != 1)
            {
                throw new InvalidOperationException($"Lobby.unity must contain exactly one LobbySceneController. Found: {controllers.Length}");
            }

            var scaler = GameObject.Find("LobbyCanvas")?.GetComponent<CanvasScaler>();
            if (scaler == null ||
                scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                scaler.referenceResolution != CanvasResolutionPolicy.ReferenceResolution ||
                scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ||
                Math.Abs(scaler.matchWidthOrHeight - CanvasResolutionPolicy.MatchWidthOrHeight) > 0.001f)
            {
                throw new InvalidOperationException("LobbyCanvas CanvasScaler does not match Phase 4.4 policy.");
            }

            var scrollRectObject = GameObject.Find("LobbyCanvas/ContentSafeArea/ScrollRect");
            var scrollRect = scrollRectObject?.GetComponent<ScrollRect>();
            if (scrollRect == null ||
                !scrollRect.vertical ||
                scrollRect.horizontal ||
                scrollRect.movementType != ScrollRect.MovementType.Clamped)
            {
                throw new InvalidOperationException("Lobby ScrollRect must be vertical-only and clamped.");
            }

            var scrollRectTransform = scrollRectObject.GetComponent<RectTransform>();
            var expectedTop = ShellLayoutMetrics.HeaderHeight + ShellLayoutMetrics.LobbyContentTopGap;
            if (Math.Abs(scrollRectTransform.offsetMax.y + expectedTop) > 0.001f ||
                Math.Abs(scrollRectTransform.offsetMin.y - ShellLayoutMetrics.LobbyBottomReserve) > 0.001f)
            {
                throw new InvalidOperationException("Lobby ScrollRect must align under the safe-area header frame.");
            }

            var carouselLayout = GameObject.Find("LobbyCanvas/ContentSafeArea/ScrollRect/Viewport/Content/CarouselSections")?.GetComponent<LayoutElement>();
            if (carouselLayout == null || carouselLayout.preferredHeight < 1000f)
            {
                throw new InvalidOperationException("CarouselSections must reserve enough preferred height for scrollable lobby content.");
            }
        }

        private static void VerifyMockData()
        {
            var data = MockCatalog.Create();
            if (data.Banners.Count != 5)
            {
                throw new InvalidOperationException($"MockCatalog must expose 5 lobby banners. Found: {data.Banners.Count}");
            }

            if (data.Categories.Count != 5)
            {
                throw new InvalidOperationException($"MockCatalog must expose 5 lobby categories. Found: {data.Categories.Count}");
            }

            var visibleCarouselCount = 0;
            var visibleGameCount = 0;
            foreach (var category in data.Categories)
            {
                if (category.Id == "all" || category.Id == "trending")
                {
                    continue;
                }

                visibleCarouselCount++;
                visibleGameCount += category.Games.Count;
            }

            if (visibleCarouselCount != 3 || visibleGameCount != 15)
            {
                throw new InvalidOperationException($"MockCatalog must expose 3 visible carousels and 15 visible cards. Found: {visibleCarouselCount} / {visibleGameCount}");
            }
        }

        private static void VerifyPureMvcBoundaries()
        {
            var uiViewSources = Directory.GetFiles(Path.Combine(Application.dataPath, "Scripts/UI/Lobby"), "*.cs", SearchOption.TopDirectoryOnly);
            foreach (var sourcePath in uiViewSources)
            {
                var source = File.ReadAllText(sourcePath);
                if (source.Contains("LobbyFacade.GetInstance", StringComparison.Ordinal) ||
                    source.Contains("SendNotification", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{sourcePath} must stay view-only and communicate through events.");
                }
            }
        }

        private static void AssertType<T>()
        {
            if (typeof(T) == null)
            {
                throw new InvalidOperationException($"Missing required type: {typeof(T).Name}");
            }
        }
    }
}
#endif
