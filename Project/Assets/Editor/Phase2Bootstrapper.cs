#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Crashmania.Config;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase2Bootstrapper
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

        [MenuItem("Crashmania/Bootstrap Phase 2")]
        public static void Run()
        {
            Directory.CreateDirectory("Assets/Scenes");

            var tokens = AssetDatabase.LoadAssetAtPath<DesignTokens>("Assets/ScriptableObjects/DesignTokens.asset");
            foreach (var sceneName in SceneNames.Where(sceneName => sceneName != "Boot"))
            {
                CreatePlaceholderScene(sceneName, tokens);
            }

            EditorBuildSettings.scenes = SceneNames
                .Select(sceneName => new EditorBuildSettingsScene($"Assets/Scenes/{sceneName}.unity", true))
                .ToArray();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase2Bootstrapper] Phase 2 scenes and build settings completed.");
        }

        private static void CreatePlaceholderScene(string sceneName, DesignTokens tokens)
        {
            var path = $"Assets/Scenes/{sceneName}.unity";
            if (File.Exists(path))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = tokens != null ? tokens.bgMain : new Color(0.157f, 0.169f, 0.220f);
            camera.orthographic = true;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var lightObject = new GameObject("Directional Light");
            lightObject.AddComponent<Light>().type = LightType.Directional;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var canvasObject = new GameObject("Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170f, 2532f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var background = CreatePanel("Background", canvasObject.transform, tokens != null ? tokens.bgMain : new Color(0.157f, 0.169f, 0.220f));
            var label = new GameObject($"{sceneName} Label");
            label.transform.SetParent(background.transform, false);
            var text = label.AddComponent<TextMeshProUGUI>();
            text.text = sceneName.ToUpperInvariant();
            text.font = tokens != null ? tokens.fontHeading : null;
            text.fontSize = 72f;
            text.fontStyle = FontStyles.Bold;
            text.color = tokens != null ? tokens.textPrimary : Color.white;
            text.alignment = TextAlignmentOptions.Center;

            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            EditorSceneManager.SaveScene(scene, path);
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = color;
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panel;
        }
    }
}
#endif
