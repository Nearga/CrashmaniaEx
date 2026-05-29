#if UNITY_EDITOR
using System;
using System.IO;
using Crashmania.PureMvc.Commands.Lobby;
using Crashmania.PureMvc.Mediators;
using Crashmania.PureMvc.Proxies;
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
            "LobbyCanvas/ScrollRect",
            "LobbyCanvas/ScrollRect/Viewport",
            "LobbyCanvas/ScrollRect/Viewport/Content",
            "LobbyCanvas/ScrollRect/Viewport/Content/PromoSection",
            "LobbyCanvas/ScrollRect/Viewport/Content/RecentMultipliers",
            "LobbyCanvas/ScrollRect/Viewport/Content/CategoryRail",
            "LobbyCanvas/ScrollRect/Viewport/Content/CarouselSections"
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
            "Assets/Resources/UI/Games/Homepage/tiltx.png"
        };

        [MenuItem("Crashmania/Verify Phase 5 Lobby")]
        public static void Run()
        {
            VerifyTypes();
            VerifyAssets();
            VerifyScene();
            VerifyPureMvcBoundaries();
            Debug.Log("[Phase5LobbyVerifier] Phase 5 lobby verification completed.");
        }

        private static void VerifyTypes()
        {
            AssertType<CatalogProxy>();
            AssertType<LoadLobbyDataCommand>();
            AssertType<LobbyMediator>();
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

            var views = UnityEngine.Object.FindObjectsByType<LobbyView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (views.Length != 1)
            {
                throw new InvalidOperationException($"Lobby.unity must contain exactly one in-scene LobbyView. Found: {views.Length}");
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
