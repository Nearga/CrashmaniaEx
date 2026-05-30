using System.IO;
using System.Linq;
using Crashmania.Boot;
using UnityEditor;
using UnityEngine;

namespace Crashmania.Editor
{
    [CustomEditor(typeof(DevSceneLoader))]
    public sealed class DevSceneLoaderEditor : UnityEditor.Editor
    {
        private SerializedProperty targetScene;
        private SerializedProperty useMock;

        private void OnEnable()
        {
            targetScene = serializedObject.FindProperty("targetScene");
            useMock = serializedObject.FindProperty("useMock");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawTargetSceneDropdown();
            EditorGUILayout.PropertyField(useMock);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTargetSceneDropdown()
        {
            var sceneNames = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToArray();

            if (sceneNames.Length == 0)
            {
                EditorGUILayout.PropertyField(targetScene);
                EditorGUILayout.HelpBox("No enabled scenes found in Build Settings.", MessageType.Warning);
                return;
            }

            var current = string.IsNullOrWhiteSpace(targetScene.stringValue)
                ? "Login"
                : targetScene.stringValue;
            var selectedIndex = System.Array.IndexOf(sceneNames, current);
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
                EditorGUILayout.HelpBox($"Current target scene '{current}' is not enabled in Build Settings.", MessageType.Info);
            }

            selectedIndex = EditorGUILayout.Popup(new GUIContent("Target Scene"), selectedIndex, sceneNames);
            targetScene.stringValue = sceneNames[selectedIndex];
        }
    }
}
