#if UNITY_EDITOR
using System;
using System.Linq;
using Crashmania.Audio;
using Crashmania.PureMvc.Commands.Navigation;
using Crashmania.PureMvc.Mediators;
using Crashmania.Services;
using Crashmania.UI.Components;
using Crashmania.UI.Shell;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase2Verifier
    {
        private static readonly string[] SceneNames =
        {
            "Boot",
            "Login",
            "Lobby",
            "Store",
            "Gifts",
            "Account",
            "Game"
        };

        [MenuItem("Crashmania/Verify Phase 2")]
        public static void Run()
        {
            VerifyTypes();
            VerifyScenes();
            VerifyBuildSettings();
            Debug.Log("[Phase2Verifier] Phase 2 verification completed.");
        }

        private static void VerifyTypes()
        {
            AssertType<NavigationService>();
            AssertType<NavigateCommand>();
            AssertType<SceneLoadedCommand>();
            AssertType<HeaderView>();
            AssertType<TabBarView>();
            AssertType<ModalView>();
            AssertType<HeaderMediator>();
            AssertType<TabBarMediator>();
            AssertType<ModalMediator>();
            AssertType<TransitionOverlay>();
            AssertType<SafeAreaPanel>();
            AssertType<AccumulateToBalance>();
            AssertType<AudioManager>();
        }

        private static void VerifyScenes()
        {
            foreach (var sceneName in SceneNames)
            {
                var path = $"Assets/Scenes/{sceneName}.unity";
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                {
                    throw new InvalidOperationException($"Missing scene: {path}");
                }

                var scene = EditorSceneManager.OpenScene(path);
                if (!scene.IsValid())
                {
                    throw new InvalidOperationException($"Could not open scene: {path}");
                }

                if (GameObject.Find("Main Camera") == null)
                {
                    throw new InvalidOperationException($"{sceneName} is missing Main Camera.");
                }

                if (GameObject.Find("Directional Light") == null)
                {
                    throw new InvalidOperationException($"{sceneName} is missing Directional Light.");
                }

                var canvasObject = GameObject.Find("Canvas");
                if (canvasObject == null || canvasObject.GetComponent<CanvasScaler>() == null)
                {
                    throw new InvalidOperationException($"{sceneName} is missing a scaled Canvas.");
                }

                if (GameObject.Find("EventSystem") == null)
                {
                    throw new InvalidOperationException($"{sceneName} is missing EventSystem.");
                }
            }
        }

        private static void VerifyBuildSettings()
        {
            var expectedPaths = SceneNames.Select(sceneName => $"Assets/Scenes/{sceneName}.unity").ToArray();
            var actualPaths = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (!expectedPaths.SequenceEqual(actualPaths))
            {
                throw new InvalidOperationException("Build Settings scene order does not match Phase 2 requirements.");
            }
        }

        private static void AssertType<T>()
        {
            if (typeof(T) == null)
            {
                throw new InvalidOperationException($"Type missing: {typeof(T).Name}");
            }
        }
    }
}
#endif
