#if UNITY_EDITOR
using Crashmania.UI.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Crashmania.Editor
{
    public static class SceneOrientationPolicySetup
    {
        private static readonly (string path, string canvasName, OrientationMode mode)[] SceneConfigs =
        {
            ("Assets/Scenes/Boot.unity", "Canvas", OrientationMode.ForcePortrait),
            ("Assets/Scenes/Login.unity", "LoginCanvas", OrientationMode.ForcePortrait),
            ("Assets/Scenes/Lobby.unity", "LobbyCanvas", OrientationMode.ForcePortrait),
            ("Assets/Scenes/Store.unity", "Canvas", OrientationMode.ForcePortrait),
            ("Assets/Scenes/Gifts.unity", "Canvas", OrientationMode.ForcePortrait),
            ("Assets/Scenes/Account.unity", "Canvas", OrientationMode.ForcePortrait),
            ("Assets/Scenes/Game.unity", "GameCanvas", OrientationMode.PortraitOrLandscape)
        };

        [MenuItem("Crashmania/Setup Scene Orientation Policies")]
        public static void Run()
        {
            foreach (var (path, canvasName, mode) in SceneConfigs)
            {
                var scene = EditorSceneManager.OpenScene(path);
                if (!scene.IsValid())
                {
                    Debug.LogError($"[SceneOrientationPolicySetup] Could not open scene: {path}");
                    continue;
                }

                // Remove any standalone [SceneOrientationPolicy] GO
                var standalone = GameObject.Find("[SceneOrientationPolicy]");
                if (standalone != null)
                {
                    Object.DestroyImmediate(standalone);
                }

                // Remove existing SceneOrientationPolicy from any object
                var existing = Object.FindAnyObjectByType<SceneOrientationPolicy>();
                if (existing != null)
                {
                    if (existing.Mode == mode && existing.GetComponent<Canvas>() != null)
                    {
                        Debug.Log($"[SceneOrientationPolicySetup] {path} already has correct {mode} policy on {existing.gameObject.name}.");
                        continue;
                    }

                    Object.DestroyImmediate(existing);
                }

                // Add to Canvas
                var canvasGO = GameObject.Find(canvasName);
                if (canvasGO == null)
                {
                    Debug.LogError($"[SceneOrientationPolicySetup] Could not find Canvas '{canvasName}' in {path}.");
                    continue;
                }

                var policy = canvasGO.AddComponent<SceneOrientationPolicy>();
                var so = new SerializedObject(policy);
                so.FindProperty("orientationMode").enumValueIndex = (int)mode;
                so.ApplyModifiedProperties();

                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[SceneOrientationPolicySetup] Added {mode} policy to {canvasName} in {path}.");
            }

            Debug.Log("[SceneOrientationPolicySetup] Done.");
        }
    }
}
#endif