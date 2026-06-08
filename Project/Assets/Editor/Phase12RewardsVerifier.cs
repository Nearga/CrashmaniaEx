#if UNITY_EDITOR
using System;
using Crashmania.PureMvc.Mediators;
using Crashmania.PureMvc.Scenes;
using Crashmania.UI.Components;
using Crashmania.UI.Game;
using Crashmania.UI.Shell;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase12RewardsVerifier
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity";
        private const string BetPanelPrefabPath = "Assets/Resources/UI/Prefabs/BetPanel.prefab";
        private const string HeaderPrefabPath = "Assets/Resources/UI/Prefabs/HeaderOverlay.prefab";

        [MenuItem("Crashmania/Verify Phase 12 Rewards")]
        public static void Run()
        {
            VerifyTypes();
            VerifyPrefabs();
            VerifyScene();
            Debug.Log("[Phase12RewardsVerifier] Phase 12 reward verification completed.");
        }

        private static void VerifyTypes()
        {
            AssertType<CurrencyRewardFlyout>();
            AssertType<GameView>();
            AssertType<GameMediator>();
        }

        private static void VerifyPrefabs()
        {
            var betPanel = AssetDatabase.LoadAssetAtPath<GameObject>(BetPanelPrefabPath);
            var panelController = betPanel != null ? betPanel.GetComponent<BetPanelController>() : null;
            if (panelController == null || panelController.RewardSource == null)
            {
                throw new InvalidOperationException("BetPanel prefab must bind a RewardSource anchor.");
            }

            var header = AssetDatabase.LoadAssetAtPath<GameObject>(HeaderPrefabPath);
            var headerView = header != null ? header.GetComponent<HeaderView>() : null;
            if (headerView == null ||
                headerView.GetRewardTarget(Crashmania.Models.CurrencyMode.CC) == null ||
                headerView.GetRewardTarget(Crashmania.Models.CurrencyMode.SC) == null)
            {
                throw new InvalidOperationException("HeaderOverlay prefab must bind CC and SC reward targets.");
            }
        }

        private static void VerifyScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath);
            var canvas = Find(scene, "GameCanvas");
            var rewardLayer = Find(scene, "GameCanvas/CurrencyRewardLayer");
            var gameView = canvas != null ? canvas.GetComponent<GameView>() : null;
            var sceneController = canvas != null ? canvas.GetComponent<GameSceneController>() : null;
            var flyout = rewardLayer != null ? rewardLayer.GetComponent<CurrencyRewardFlyout>() : null;

            if (canvas == null || canvas.GetComponentsInChildren<Canvas>(true).Length != 1)
            {
                throw new InvalidOperationException("Game scene must retain exactly one Canvas.");
            }

            if (rewardLayer == null || rewardLayer.GetComponent<Canvas>() != null ||
                rewardLayer.GetComponent<GraphicRaycaster>() != null)
            {
                throw new InvalidOperationException("CurrencyRewardLayer must be scene-owned and raycast-free without another Canvas.");
            }

            if (flyout == null || flyout.CcCoinSprite == null || flyout.ScCoinSprite == null ||
                flyout.RequiredPoolCapacity < 16)
            {
                throw new InvalidOperationException("CurrencyRewardFlyout must bind both coin sprites and support two maximum bursts.");
            }

            if (gameView == null || sceneController == null)
            {
                throw new InvalidOperationException("GameCanvas must bind GameView and GameSceneController.");
            }

            var panels = canvas.GetComponentsInChildren<BetPanelController>(true);
            if (panels.Length != 2 || panels[0].RewardSource == null || panels[1].RewardSource == null)
            {
                throw new InvalidOperationException("Both scene bet panels must expose reward sources.");
            }

            var header = canvas.GetComponentInChildren<HeaderView>(true);
            if (header == null ||
                header.GetRewardTarget(Crashmania.Models.CurrencyMode.CC) == null ||
                header.GetRewardTarget(Crashmania.Models.CurrencyMode.SC) == null)
            {
                throw new InvalidOperationException("Game header must expose both reward targets.");
            }

            foreach (var panel in panels)
            {
                if (PrefabUtility.GetPrefabInstanceStatus(panel.gameObject) != PrefabInstanceStatus.Connected)
                {
                    throw new InvalidOperationException($"{panel.name} must remain connected to BetPanel.prefab.");
                }
            }

            if (PrefabUtility.GetPrefabInstanceStatus(header.gameObject) != PrefabInstanceStatus.Connected)
            {
                throw new InvalidOperationException("Game HeaderOverlay must remain connected to the shared prefab.");
            }
        }

        private static GameObject Find(Scene scene, string path)
        {
            var parts = path.Split('/');
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != parts[0])
                {
                    continue;
                }

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

        private static void AssertType<T>()
        {
            if (typeof(T) == null)
            {
                throw new InvalidOperationException("Missing required Phase 12 type.");
            }
        }
    }
}
#endif
