using System;
using System.Linq;
using Crashmania.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace Crashmania.Services
{
    public sealed class EmbeddedGameLoader : IGameLoader
    {
        private Scene loadedGameScene;
        private string previousSceneName;

        public async UniTask LoadGame(GameModel game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            var sceneName = string.IsNullOrWhiteSpace(game.SceneAddress) ? "Game" : game.SceneAddress.Trim();
            previousSceneName = SceneManager.GetActiveScene().name;

            var existing = SceneManager.GetSceneByName(sceneName);
            if (!existing.IsValid() || !existing.isLoaded)
            {
                var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                if (operation != null)
                {
                    await operation.ToUniTask();
                }

                existing = SceneManager.GetSceneByName(sceneName);
            }

            if (!existing.IsValid() || !existing.isLoaded)
            {
                throw new InvalidOperationException($"Game scene '{sceneName}' could not be loaded.");
            }

            loadedGameScene = existing;
            SanitizeAdditiveSceneServices(loadedGameScene);
            SceneManager.SetActiveScene(loadedGameScene);
        }

        public async UniTask UnloadGame()
        {
            var sceneToUnload = ResolveLoadedGameScene();
            if (!sceneToUnload.IsValid() || !sceneToUnload.isLoaded)
            {
                return;
            }

            var previousScene = SceneManager.GetSceneByName(previousSceneName);
            if (previousScene.IsValid() && previousScene.isLoaded)
            {
                SceneManager.SetActiveScene(previousScene);
            }
            else
            {
                var fallbackScene = Enumerable.Range(0, SceneManager.sceneCount)
                    .Select(SceneManager.GetSceneAt)
                    .FirstOrDefault(scene => scene.isLoaded && scene != sceneToUnload);

                if (fallbackScene.IsValid() && fallbackScene.isLoaded)
                {
                    SceneManager.SetActiveScene(fallbackScene);
                }
            }

            if (SceneManager.sceneCount > 1)
            {
                var operation = SceneManager.UnloadSceneAsync(sceneToUnload);
                if (operation != null)
                {
                    await operation.ToUniTask();
                }
            }

            loadedGameScene = default;
        }

        private static void SanitizeAdditiveSceneServices(Scene gameScene)
        {
            var eventSystems = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude);
            var hasExternalEventSystem = eventSystems.Any(system => system.gameObject.scene != gameScene && system.isActiveAndEnabled);
            foreach (var system in eventSystems)
            {
                if (hasExternalEventSystem && system.gameObject.scene == gameScene)
                {
                    system.gameObject.SetActive(false);
                }
            }

            var audioListeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude);
            var hasExternalAudioListener = audioListeners.Any(listener => listener.gameObject.scene != gameScene && listener.isActiveAndEnabled);
            foreach (var listener in audioListeners)
            {
                if (hasExternalAudioListener && listener.gameObject.scene == gameScene)
                {
                    listener.enabled = false;
                }
            }
        }

        private Scene ResolveLoadedGameScene()
        {
            if (loadedGameScene.IsValid() && loadedGameScene.isLoaded)
            {
                return loadedGameScene;
            }

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.name == "Game")
            {
                return activeScene;
            }

            return SceneManager.GetSceneByName("Game");
        }
    }
}
