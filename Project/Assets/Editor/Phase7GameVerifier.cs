#if UNITY_EDITOR
using System;
using System.IO;
using Crashmania.Game;
using Crashmania.Models;
using Crashmania.PureMvc.Commands.Game;
using Crashmania.PureMvc.Commands.Lobby;
using Crashmania.PureMvc.Proxies;
using Crashmania.PureMvc.Scenes;
using Crashmania.Services;
using Crashmania.UI.Components;
using Crashmania.UI.Game;
using TMPro;
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

        private static readonly string[] ApprovedFontAssetPaths =
        {
            "Assets/UI/Fonts/TMP/Murecho-Regular SDF.asset",
            "Assets/UI/Fonts/TMP/Murecho-SemiBold SDF.asset",
            "Assets/UI/Fonts/TMP/Murecho-Bold SDF.asset",
            "Assets/UI/Fonts/TMP/Murecho-Black SDF.asset",
            "Assets/UI/Fonts/TMP/SairaCondensed-Black SDF.asset"
        };

        private static readonly string[] RequiredScenePaths =
        {
            "GameCanvas",
            "GameCanvas/SafeAreaPanel",
            "GameCanvas/SafeAreaPanel/Header Bar",
            "GameCanvas/SafeAreaPanel/GameViewportContainer",
            "GameCanvas/SafeAreaPanel/GameViewportContainer/HistoryContent",
            "GameCanvas/SafeAreaPanel/GameViewportContainer/Rocket",
            "GameCanvas/SafeAreaPanel/DualBetContainer",
            "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_A",
            "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_B",
            "GameCanvas/SafeAreaPanel/DualBetContainer/ActiveBetsAccordion",
            "GameCanvas/SafeAreaPanel/DualBetContainer/ActiveBetsAccordion/ScrollArea/PlayerRowsContent"
        };

        [MenuItem("Crashmania/Verify Phase 7 Game")]
        public static void Run()
        {
            VerifyTypes();
            VerifyAssets();
            VerifyScene();
            VerifyAutoplay();
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
            AssertType<CrashRocketAnimator>();
            AssertType<CrashBackgroundAnimator>();
            AssertType<BetPanelController>();
            AssertType(typeof(AutoplaySettings));
        }

        private static void VerifyAssets()
        {
            AssertApprovedFontAssets();

            var betPanelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BetPanelPrefabPath);
            if (betPanelPrefab == null)
            {
                throw new InvalidOperationException($"Missing required game prefab: {BetPanelPrefabPath}");
            }

            if (betPanelPrefab.GetComponent<BetPanelController>() == null)
            {
                throw new InvalidOperationException("BetPanel prefab must have BetPanelController on its root.");
            }

            AssertImportedSprite("RocketDreams.asset");
            AssertImportedSprite("Top Bar-coin_crash.asset");
            AssertImportedSprite("Top Bar-coin_sweep.asset");
            AssertPrefabTextFonts(betPanelPrefab, BetPanelPrefabPath);
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

            Canvas.ForceUpdateCanvases();
            var dualBetContainer = FindSceneObject(scene, "GameCanvas/SafeAreaPanel/DualBetContainer")?.GetComponent<RectTransform>();
            if (dualBetContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(dualBetContainer);
            }

            AssertTopRect(scene, "GameCanvas/SafeAreaPanel/Header Bar", 0f, 160f, 36f);
            AssertTopRect(scene, "GameCanvas/SafeAreaPanel/GameViewportContainer", 160f, 807f, 36f);
            AssertTopRect(scene, "GameCanvas/SafeAreaPanel/DualBetContainer", 967f, 1114f, 42f);
            AssertTopRect(scene, "GameCanvas/SafeAreaPanel/DualBetContainer/ActiveBetsAccordion", 728f, 386f, 42f);
            
            AssertNonEmptyImage(scene, "GameCanvas/SafeAreaPanel/Header Bar");
            AssertNonEmptyImage(scene, "GameCanvas/SafeAreaPanel/GameViewportContainer");
            AssertNonEmptyImage(scene, "GameCanvas/SafeAreaPanel/DualBetContainer/ActiveBetsAccordion");
            AssertNonEmptyImage(scene, "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_A");
            AssertNonEmptyImage(scene, "GameCanvas/SafeAreaPanel/DualBetContainer/BetPanel_B");

            var rocketImage = FindSceneObject(scene, "GameCanvas/SafeAreaPanel/GameViewportContainer/Rocket")?.GetComponent<Image>();
            if (rocketImage == null || rocketImage.sprite == null || !rocketImage.preserveAspect)
            {
                throw new InvalidOperationException("Rocket must use an aspect-preserved sprite image.");
            }

            AssertAnimationFidelityObjects(scene, canvas);
            AssertSceneTextFonts(scene, canvas);

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

        private static void AssertApprovedFontAssets()
        {
            foreach (var path in ApprovedFontAssetPaths)
            {
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (font == null)
                {
                    throw new InvalidOperationException($"Missing approved TMP font asset: {path}");
                }

                AssertFontAssetHealth(font, path);
                AssertFontCanGenerateMesh(font, path);
            }
        }

        private static void AssertFontAssetHealth(TMP_FontAsset font, string context)
        {
            if (font.atlasTextures == null || font.atlasTextures.Length == 0 || font.atlasTextures[0] == null)
            {
                throw new InvalidOperationException($"{context} has no valid TMP atlas texture.");
            }

            if (font.material == null)
            {
                throw new InvalidOperationException($"{context} has no TMP material.");
            }

            if (!font.material.HasProperty("_MainTex") || font.material.GetTexture("_MainTex") == null)
            {
                throw new InvalidOperationException($"{context} TMP material has no _MainTex.");
            }

            if (font.characterTable == null || font.characterTable.Count == 0 ||
                font.glyphTable == null || font.glyphTable.Count == 0)
            {
                throw new InvalidOperationException($"{context} has no TMP character/glyph data.");
            }
        }

        private static void AssertFontCanGenerateMesh(TMP_FontAsset font, string context)
        {
            var canvasObject = new GameObject("TMP Font Sanity Canvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.hideFlags = HideFlags.HideAndDontSave;
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var textObject = new GameObject("TMP Font Sanity Text", typeof(RectTransform));
            textObject.hideFlags = HideFlags.HideAndDontSave;
            textObject.transform.SetParent(canvasObject.transform, false);

            try
            {
                var text = textObject.AddComponent<TextMeshProUGUI>();
                text.font = font;
                text.fontSharedMaterial = font.material;
                text.text = "ABC123 1.25x BET + -";
                text.fontSize = 32f;
                text.rectTransform.sizeDelta = new Vector2(600f, 120f);
                text.ForceMeshUpdate(true, true);

                if (text.mesh == null || text.mesh.vertexCount == 0)
                {
                    throw new InvalidOperationException($"{context} failed TMP mesh generation.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(textObject);
                UnityEngine.Object.DestroyImmediate(canvasObject);
            }
        }

        private static void AssertPrefabTextFonts(GameObject prefab, string context)
        {
            foreach (var text in prefab.GetComponentsInChildren<TMP_Text>(true))
            {
                AssertTextUsesApprovedFont(text, $"{context}/{GetTransformPath(text.transform, prefab.transform)}", validateMesh: false);
            }
        }

        private static void AssertSceneTextFonts(Scene scene, GameObject canvas)
        {
            foreach (var text in canvas.GetComponentsInChildren<TMP_Text>(true))
            {
                AssertTextUsesApprovedFont(text, $"{scene.path}/{GetTransformPath(text.transform, canvas.transform)}", validateMesh: true);
            }
        }

        private static void AssertTextUsesApprovedFont(TMP_Text text, string context, bool validateMesh)
        {
            if (text.font == null)
            {
                throw new InvalidOperationException($"{context} has no TMP font asset.");
            }

            var path = AssetDatabase.GetAssetPath(text.font);
            if (Array.IndexOf(ApprovedFontAssetPaths, path) < 0)
            {
                throw new InvalidOperationException($"{context} uses non-approved TMP font '{text.font.name}' at '{path}'.");
            }

            AssertFontAssetHealth(text.font, context);

            if (text.fontSharedMaterial == null)
            {
                throw new InvalidOperationException($"{context} has no TMP shared material assigned.");
            }

            if (!text.fontSharedMaterial.HasProperty("_MainTex") || text.fontSharedMaterial.GetTexture("_MainTex") == null)
            {
                throw new InvalidOperationException($"{context} TMP shared material has no _MainTex.");
            }

            if (!validateMesh || !text.gameObject.activeInHierarchy || string.IsNullOrEmpty(text.text))
            {
                return;
            }

            try
            {
                text.ForceMeshUpdate(true, true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{context} failed TMP mesh update: {ex.Message}", ex);
            }

            if (text.mesh == null || text.mesh.vertexCount == 0)
            {
                throw new InvalidOperationException($"{context} generated an empty TMP mesh.");
            }
        }

        private static string GetTransformPath(Transform target, Transform root)
        {
            var path = target.name;
            var current = target.parent;
            while (current != null && current != root.parent)
            {
                path = current.name + "/" + path;
                if (current == root)
                {
                    break;
                }

                current = current.parent;
            }

            return path;
        }

        private static void VerifyAutoplay()
        {
            // Verify AutoplaySettings model fields
            var settings = new AutoplaySettings();
            if (!settings.Enabled)
            {
                // Default should be disabled — that's correct
            }

            if (AutoplaySettings.RoundCounts.Length != 5)
            {
                throw new InvalidOperationException("AutoplaySettings.RoundCounts must have 5 entries (∞, 10, 25, 50, 100).");
            }

            if (AutoplaySettings.RoundCounts[0] != -1)
            {
                throw new InvalidOperationException("AutoplaySettings.RoundCounts[0] must be -1 (infinite).");
            }

            if (AutoplaySettings.MinCashOutMultiplier > 1.1 || AutoplaySettings.MaxCashOutMultiplier < 100.0)
            {
                throw new InvalidOperationException("AutoplaySettings cash-out multiplier bounds are incorrect.");
            }

            // Verify CrashPlayerBet has AutoCashOutMultiplier field
            var bet = new CrashPlayerBet();
            bet.AutoCashOutMultiplier = 2.5;
            if (bet.AutoCashOutMultiplier != 2.5)
            {
                throw new InvalidOperationException("CrashPlayerBet.AutoCashOutMultiplier must be settable.");
            }

            // Verify BetPanelController has ResetAutoplay method
            var resetMethod = typeof(BetPanelController).GetMethod("ResetAutoplay");
            if (resetMethod == null)
            {
                throw new InvalidOperationException("BetPanelController must have a public ResetAutoplay method for Phase 11.2.");
            }

            // Verify Autoplay property exists
            var autoplayProp = typeof(BetPanelController).GetProperty("Autoplay");
            if (autoplayProp == null || autoplayProp.PropertyType != typeof(AutoplaySettings))
            {
                throw new InvalidOperationException("BetPanelController must have an Autoplay property of type AutoplaySettings.");
            }

            Debug.Log("[Phase7GameVerifier] Phase 11.2 autoplay verification passed.");
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

        private static void AssertAnimationFidelityObjects(Scene scene, GameObject canvas)
        {
            var rocket = FindSceneObject(scene, "GameCanvas/SafeAreaPanel/GameViewportContainer/Rocket");
            if (rocket == null || rocket.GetComponent<CrashRocketAnimator>() == null)
            {
                throw new InvalidOperationException("Rocket must carry CrashRocketAnimator for Phase 7.11 animation fidelity recovery.");
            }

            if (canvas.GetComponent<CrashBackgroundAnimator>() == null)
            {
                throw new InvalidOperationException("GameCanvas must carry CrashBackgroundAnimator for Phase 7.11 layered background motion.");
            }

            var requiredLayers = new[]
            {
                "GameCanvas/SafeAreaPanel/GameViewportContainer/CountdownBackground",
                "GameCanvas/SafeAreaPanel/GameViewportContainer/FlightSpaceBackground",
                "GameCanvas/SafeAreaPanel/GameViewportContainer/Asteroids",
                "GameCanvas/SafeAreaPanel/GameViewportContainer/Stars",
                "GameCanvas/SafeAreaPanel/GameViewportContainer/Planet",
                "GameCanvas/SafeAreaPanel/GameViewportContainer/GroundOrMoonLayer",
                "GameCanvas/SafeAreaPanel/GameViewportContainer/SpeedLines",
                "GameCanvas/SafeAreaPanel/GameViewportContainer/CrashTint",
                "GameCanvas/SafeAreaPanel/GameViewportContainer/Rocket/RocketGlow"
            };

            foreach (var path in requiredLayers)
            {
                if (FindSceneObject(scene, path) == null)
                {
                    throw new InvalidOperationException($"Missing Phase 7.11 animation layer: {path}");
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

        private static void AssertImportedSprite(string assetName)
        {
            var path = ExtractedSpriteRoot + assetName;
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null)
            {
                throw new InvalidOperationException($"Missing imported reference sprite: {path}");
            }
        }

        private static void AssertTopRect(Scene scene, string path, float expectedTop, float expectedHeight, float tolerance)
        {
            var rt = FindSceneObject(scene, path)?.GetComponent<RectTransform>();
            if (rt == null)
            {
                throw new InvalidOperationException($"{path} is missing RectTransform.");
            }

            var top = -rt.anchoredPosition.y;
            if (Math.Abs(top - expectedTop) > tolerance || Math.Abs(rt.sizeDelta.y - expectedHeight) > tolerance)
            {
                throw new InvalidOperationException($"{path} is outside expected screenshot band. top={top:0.#}, height={rt.sizeDelta.y:0.#}");
            }
        }

        private static void AssertNonEmptyImage(Scene scene, string path)
        {
            var image = FindSceneObject(scene, path)?.GetComponent<Image>();
            if (image == null || image.color.a < 0.05f)
            {
                throw new InvalidOperationException($"{path} must have a visible Image surface.");
            }
        }
    }
}
#endif
