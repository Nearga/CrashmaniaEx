#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Crashmania.Game;
using Crashmania.PureMvc.Scenes;
using Crashmania.UI.Components;
using Crashmania.UI.Game;
using Crashmania.UI.Lobby;
using Crashmania.UI.Shell;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Crashmania.Editor
{
    public static class ExplicitSceneWiringVerifier
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/Lobby.unity",
            "Assets/Scenes/Game.unity"
        };

        private static readonly string[] PrefabPaths =
        {
            "Assets/Resources/UI/Prefabs/CategoryChip.prefab",
            "Assets/Resources/UI/Prefabs/GamesCarousel.prefab",
            "Assets/Resources/UI/Prefabs/GameCard.prefab",
            "Assets/Resources/UI/Prefabs/GameCardTop10.prefab",
            "Assets/Resources/UI/Prefabs/HeaderOverlay.prefab",
            "Assets/Resources/UI/Prefabs/ModalManagerOverlay.prefab",
            "Assets/Resources/UI/Prefabs/TabBarOverlay.prefab",
            "Assets/Resources/UI/Prefabs/ToastOverlay.prefab",
            "Assets/Resources/UI/Prefabs/StoreItemCard.prefab",
            "Assets/Resources/UI/Prefabs/BetPanel.prefab"
        };

        private static readonly HashSet<Type> AuditedTypes = new()
        {
            typeof(LobbyView),
            typeof(LobbySceneController),
            typeof(PromoBannerView),
            typeof(RecentMultipliersView),
            typeof(StorePanelView),
            typeof(StoreItemCardView),
            typeof(CategoryChipView),
            typeof(GameCardView),
            typeof(GamesCarouselView),
            typeof(HeaderView),
            typeof(TabBarView),
            typeof(ModalView),
            typeof(ToastView),
            typeof(GameSceneController),
            typeof(CrashGameController),
            typeof(CrashRocketAnimator),
            typeof(CrashBackgroundAnimator),
            typeof(ScrollingGridBackground),
            typeof(BetPanelController)
        };

        private static readonly HashSet<string> OptionalProperties = new()
        {
            $"{nameof(PromoBannerView)}.titleText",
            $"{nameof(PromoBannerView)}.ctaButton",
            $"{nameof(CrashRocketAnimator)}.optionalSpineRoot"
        };

        private static readonly string[] AuditedSourcePaths =
        {
            "Assets/Scripts/UI/Lobby/LobbyView.cs",
            "Assets/Scripts/UI/Lobby/RecentMultipliersView.cs",
            "Assets/Scripts/UI/Lobby/PromoBannerView.cs",
            "Assets/Scripts/UI/Lobby/CategoryChipView.cs",
            "Assets/Scripts/UI/Lobby/GameCardView.cs",
            "Assets/Scripts/UI/Lobby/GamesCarouselView.cs",
            "Assets/Scripts/UI/Shell/HeaderView.cs",
            "Assets/Scripts/UI/Shell/ShellBootstrapper.cs",
            "Assets/Scripts/UI/Shell/TabBarView.cs",
            "Assets/Scripts/UI/Shell/ModalView.cs",
            "Assets/Scripts/UI/Shell/ToastView.cs",
            "Assets/Scripts/Game/CrashGameController.cs",
            "Assets/Scripts/Game/CrashRocketAnimator.cs",
            "Assets/Scripts/UI/Game/BetPanelController.cs",
            "Assets/Scripts/UI/Components/ScrollingGridBackground.cs",
            "Assets/Scripts/PureMvc/Scenes/LobbySceneController.cs",
            "Assets/Scripts/PureMvc/Scenes/GameSceneController.cs"
        };

        private static readonly string[] ForbiddenSourceFragments =
        {
            "transform.Find(",
            "FindDeep<",
            "FindAnyObjectByType<Crashmania.UI.Shell.HeaderView",
            "GetComponentInChildren<IGameController>"
        };

        [MenuItem("Crashmania/Verify Explicit Scene Wiring")]
        public static void Run()
        {
            VerifySources();

            foreach (var path in PrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    throw new InvalidOperationException($"Missing audited prefab: {path}");
                }

                VerifyHierarchy(prefab, path, int.MaxValue);
            }

            foreach (var path in ScenePaths)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                foreach (var root in scene.GetRootGameObjects())
                {
                    VerifyHierarchy(root, path, 5);
                }

                EditorSceneManager.CloseScene(scene, true);
            }

            Debug.Log("[ExplicitSceneWiringVerifier] Lobby, Game, and direct prefab wiring passed.");
        }

        private static void VerifySources()
        {
            foreach (var assetPath in AuditedSourcePaths)
            {
                var fullPath = Path.GetFullPath(assetPath);
                var source = File.ReadAllText(fullPath);
                foreach (var fragment in ForbiddenSourceFragments)
                {
                    if (source.Contains(fragment, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"{assetPath} still relies on runtime discovery: {fragment}");
                    }
                }
            }
        }

        private static void VerifyHierarchy(GameObject root, string context, int maxDepth)
        {
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!AuditedTypes.Contains(behaviour.GetType()) || GetDepth(behaviour.transform, root.transform) > maxDepth)
                {
                    continue;
                }

                VerifyComponent(behaviour, context, root.transform);
            }
        }

        private static void VerifyComponent(MonoBehaviour behaviour, string context, Transform root)
        {
            var serializedObject = new SerializedObject(behaviour);
            var property = serializedObject.GetIterator();
            while (property.NextVisible(true))
            {
                if (property.propertyPath == "m_Script" ||
                    property.propertyType != SerializedPropertyType.ObjectReference ||
                    OptionalProperties.Contains($"{behaviour.GetType().Name}.{property.propertyPath}"))
                {
                    continue;
                }

                if (property.objectReferenceValue == null)
                {
                    throw new InvalidOperationException(
                        $"{context}/{GetPath(behaviour.transform, root)} has unwired {behaviour.GetType().Name}.{property.propertyPath}");
                }
            }
        }

        private static int GetDepth(Transform target, Transform root)
        {
            var depth = 0;
            while (target != root && target.parent != null)
            {
                depth++;
                target = target.parent;
            }

            return depth;
        }

        private static string GetPath(Transform target, Transform root)
        {
            var path = target.name;
            while (target != root && target.parent != null)
            {
                target = target.parent;
                path = target.name + "/" + path;
            }

            return path;
        }
    }
}
#endif
