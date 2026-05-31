#if UNITY_EDITOR
using System;
using System.IO;
using Crashmania.Game;
using Crashmania.PureMvc.Commands.Game;
using Crashmania.PureMvc.Commands.Lobby;
using Crashmania.PureMvc.Proxies;
using Crashmania.PureMvc.Scenes;
using Crashmania.Services;
using Crashmania.UI.Components;
using Crashmania.UI.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase7GameVerifier
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity";
                private const string ExtractedSpriteRoot = "Assets/Resources/UI/Game/Extracted/Sprite/";

        private static readonly string[] RequiredArtAssetPaths =
        {
            ExtractedSpriteRoot + "Crash_mode_BG_default.asset",
            ExtractedSpriteRoot + "RocketDreams.asset",
            ExtractedSpriteRoot + "rocket-start 1.asset",
            ExtractedSpriteRoot + "bet_ui_container.asset",
            ExtractedSpriteRoot + "Bet amount.asset",
            ExtractedSpriteRoot + "round_history_bg.asset",
            ExtractedSpriteRoot + "ButtonGrey.asset",
            ExtractedSpriteRoot + "ButtonRed.asset",
            ExtractedSpriteRoot + "CancelButton.asset",
            ExtractedSpriteRoot + "ChangeBetButton.asset",
            ExtractedSpriteRoot + "Top Bar-text_field.asset",
            ExtractedSpriteRoot + "Top Bar-toggle_bar_background.asset",
            ExtractedSpriteRoot + "Top Bar-coin_crash.asset",
            ExtractedSpriteRoot + "Top Bar-coin_sweep.asset"
        };

private const string BetPanelPrefabPath = "Assets/Resources/UI/Prefabs/BetPanel.prefab";

        private static readonly string[] RequiredScenePaths =
        {
            "GameCanvas",
            "GameCanvas/SafeAreaPanel",
            "GameCanvas/SafeAreaPanel/GameHeader",
            "GameCanvas/SafeAreaPanel/ViewportContainer",
            "GameCanvas/SafeAreaPanel/ViewportContainer/HistoryContent",
            "GameCanvas/SafeAreaPanel/ViewportContainer/Rocket",
            "GameCanvas/SafeAreaPanel/ActiveBetsAccordion",
            "GameCanvas/SafeAreaPanel/ActiveBetsAccordion/PlayerRowsContent",
            "GameCanvas/SafeAreaPanel/DualBetContainer",
            "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_A",
            "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_B"
        };

        [MenuItem("Crashmania/Verify Phase 7 Game")]
        public static void Run()
        {
            VerifyTypes();
            VerifyAssets();
            VerifyScene();
            VerifyPureMvcBoundaries();
            Debug.Log("[Phase7GameVerifier] Phase 7 game verification completed.");
        }

        private static void VerifyTypes()
        {
            AssertType<IGameLoader>();
            AssertType<EmbeddedGameLoader>();
            AssertType<ICrashGameService>();
            AssertType<ActiveGameProxy>();
            AssertType<LaunchGameCommand>();
            AssertType<ExitGameCommand>();
            AssertType<GameSceneController>();
            AssertType<IGameController>();
            AssertType<CrashGameController>();
            AssertType(typeof(CrashCurveEvaluator));
            AssertType<ScrollingGridBackground>();
            AssertType<BetPanelController>();
        }

private static void VerifyAssets()
        {
            foreach (var artPath in RequiredArtAssetPaths)
            {
                if (AssetDatabase.LoadAssetAtPath<Sprite>(artPath) == null)
                {
                    throw new InvalidOperationException($"Missing required extracted game art sprite: {artPath}");
                }
            }

            var betPanelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BetPanelPrefabPath);
            if (betPanelPrefab == null)
            {
                throw new InvalidOperationException($"Missing required game prefab: {BetPanelPrefabPath}");
            }

            if (betPanelPrefab.GetComponent<BetPanelController>() == null)
            {
                throw new InvalidOperationException("BetPanel prefab must have BetPanelController on its root.");
            }

            var prefabImage = betPanelPrefab.GetComponent<Image>();
            if (prefabImage == null || prefabImage.sprite == null || prefabImage.sprite.name != "bet_ui_container")
            {
                throw new InvalidOperationException("BetPanel prefab root must use the extracted bet_ui_container sprite.");
            }

            var actionButton = betPanelPrefab.transform.Find("ActionButton")?.GetComponent<Image>();
            if (actionButton == null || actionButton.sprite == null || actionButton.sprite.name != "ButtonRed")
            {
                throw new InvalidOperationException("BetPanel prefab ActionButton must use the extracted ButtonRed sprite.");
            }

            var amountField = betPanelPrefab.transform.Find("AmountField")?.GetComponent<Image>();
            if (amountField == null || amountField.sprite == null || amountField.sprite.name != "Bet amount")
            {
                throw new InvalidOperationException("BetPanel prefab must include an AmountField using the extracted Bet amount sprite.");
            }
        }

private static void VerifyScene()
        {
            if (!IsSceneInBuildSettings(GameScenePath))
            {
                throw new InvalidOperationException("Game.unity must be enabled in Build Settings.");
            }

            var scene = SceneManager.GetSceneByPath(GameScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Additive);
            }

            if (!scene.IsValid())
            {
                throw new InvalidOperationException($"Could not open scene: {GameScenePath}");
            }

            SceneManager.SetActiveScene(scene);

            foreach (var path in RequiredScenePaths)
            {
                if (FindSceneObject(scene, path) == null)
                {
                    throw new InvalidOperationException($"{GameScenePath} is missing required object: {path}");
                }
            }

            var canvas = FindSceneObject(scene, "GameCanvas");
            var scaler = canvas != null ? canvas.GetComponent<CanvasScaler>() : null;
            if (scaler == null ||
                scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                scaler.referenceResolution != CanvasResolutionPolicy.ReferenceResolution ||
                scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ||
                Math.Abs(scaler.matchWidthOrHeight - CanvasResolutionPolicy.MatchWidthOrHeight) > 0.001f)
            {
                throw new InvalidOperationException("GameCanvas CanvasScaler does not match the project resolution policy.");
            }

            if (canvas.GetComponent<GameSceneController>() == null || canvas.GetComponent<CrashGameController>() == null)
            {
                throw new InvalidOperationException("GameCanvas must have GameSceneController and CrashGameController.");
            }

            var betPanels = canvas.GetComponentsInChildren<BetPanelController>(true);
            if (betPanels.Length != 2)
            {
                throw new InvalidOperationException($"Game scene must contain exactly two BetPanelController instances. Found: {betPanels.Length}");
            }

            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/GameHeader", "Top Bar-top_bar_crash_container");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/GameHeader/CCBalance", "Top Bar-text_field");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/GameHeader/SCBalance", "Top Bar-text_field");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/GameHeader/CurrencyToggleButton", "Top Bar-toggle_bar_background");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/ViewportContainer", "Crash_mode_BG_default");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/ViewportContainer/Rocket", "RocketDreams");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/ViewportContainer/Explosion", "rocket-start 1");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/ActiveBetsAccordion", "bet_ui_container");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_A", "bet_ui_container");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_A/ActionButton", "ButtonRed");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_A/AmountField", "Bet amount");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_B", "bet_ui_container");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_B/ActionButton", "ButtonRed");
            AssertImageSprite(scene, "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_B/AmountField", "Bet amount");
            AssertImageSprite(scene, "GameCanvas/HistoryBadgeTemplate", "round_history_bg");

            var eventSystemCount = 0;
            var audioListenerCount = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                eventSystemCount += root.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true).Length;
                audioListenerCount += root.GetComponentsInChildren<AudioListener>(true).Length;
            }

            if (eventSystemCount > 1)
            {
                throw new InvalidOperationException($"Game scene must not contain duplicate EventSystems. Found: {eventSystemCount}");
            }

            if (audioListenerCount > 1)
            {
                throw new InvalidOperationException($"Game scene must not contain duplicate AudioListeners. Found: {audioListenerCount}");
            }
        }

        private static void VerifyPureMvcBoundaries()
        {
            var uiRoot = Path.Combine(Application.dataPath, "Scripts/UI/Game");
            if (!Directory.Exists(uiRoot))
            {
                throw new InvalidOperationException("Missing UI/Game script folder.");
            }

            foreach (var sourcePath in Directory.GetFiles(uiRoot, "*.cs", SearchOption.TopDirectoryOnly))
            {
                var source = File.ReadAllText(sourcePath);
                if (source.Contains("LobbyFacade.GetInstance", StringComparison.Ordinal) ||
                    source.Contains("SendNotification", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{sourcePath} must stay view-only and communicate through events.");
                }
            }
        }

        private static bool IsSceneInBuildSettings(string scenePath)
        {
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && scene.path == scenePath)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AssertType<T>()
        {
            AssertType(typeof(T));
        }

        private static void AssertType(Type type)
        {
            if (type == null)
            {
                throw new InvalidOperationException("Missing required type.");
            }
        }
    

private static GameObject FindSceneObject(Scene scene, string path)
        {
            var parts = path.Split('/');
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != parts[0]) continue;

                var current = root.transform;
                for (var i = 1; i < parts.Length; i++)
                {
                    current = current.Find(parts[i]);
                    if (current == null)
                    {
                        return null;
                    }
                }

                return current.gameObject;
            }

            return null;
        }

        private static void AssertImageSprite(Scene scene, string path, string spriteName)
        {
            var target = FindSceneObject(scene, path);
            var image = target != null ? target.GetComponent<Image>() : null;
            if (image == null || image.sprite == null || image.sprite.name != spriteName)
            {
                throw new InvalidOperationException($"{path} must use extracted sprite '{spriteName}'.");
            }
        }
}
}
#endif
