#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using Crashmania.Config;
using Crashmania.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase1Bootstrapper
    {
        private const string AppConfigPath = "Assets/Resources/AppConfig.asset";
        private const string DesignTokensPath = "Assets/ScriptableObjects/DesignTokens.asset";
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private const string PipelineAssetPath = "Assets/Settings/Crashmania_URP_2D.asset";
        private const string RendererAssetPath = "Assets/Settings/Crashmania_2D_Renderer.asset";

        [MenuItem("Crashmania/Bootstrap Phase 1")]
        public static void Run()
        {
            EnsureFolders();
            AssetDatabase.Refresh();

            ImportTmpEssentials();
            CreateFontAssets();
            var appConfig = LoadOrCreate<AppConfig>(AppConfigPath);
            var designTokens = LoadOrCreate<DesignTokens>(DesignTokensPath);
            AssignDesignFonts(designTokens);
            ConfigureDotweenDefaults();

            ConfigureIosPlayerSettings();
            ConfigureUrp2D();
            CreateBootScene(designTokens);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase1Bootstrapper] Phase 1 bootstrap completed.");
        }

        private static void EnsureFolders()
        {
            foreach (var path in new[]
                     {
                         "Assets/Resources",
                         "Assets/Scenes",
                         "Assets/ScriptableObjects",
                         "Assets/Settings",
                         "Assets/UI",
                         "Assets/UI/Fonts",
                         "Assets/UI/Fonts/TMP"
                     })
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void ImportTmpEssentials()
        {
            if (AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
            {
                return;
            }

            try
            {
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Settings).Assembly);
                if (packageInfo == null)
                {
                    throw new InvalidOperationException("Could not locate the TMP package path.");
                }

                var packagePath = packageInfo.resolvedPath;
                var essentialsPath = Path.Combine(packagePath, "Package Resources/TMP Essential Resources.unitypackage");
                AssetDatabase.ImportPackage(essentialsPath, interactive: false);
                AssetDatabase.Refresh();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Phase1Bootstrapper] TMP essentials import failed: {exception.Message}");
            }
        }

        private static void CreateFontAssets()
        {
            CreateFontAsset("Assets/UI/Fonts/Murecho-Regular.ttf", "Assets/UI/Fonts/TMP/Murecho-Regular SDF.asset");
            CreateFontAsset("Assets/UI/Fonts/Murecho-SemiBold.ttf", "Assets/UI/Fonts/TMP/Murecho-SemiBold SDF.asset");
            CreateFontAsset("Assets/UI/Fonts/Murecho-Bold.ttf", "Assets/UI/Fonts/TMP/Murecho-Bold SDF.asset");
            CreateFontAsset("Assets/UI/Fonts/Murecho-Black.ttf", "Assets/UI/Fonts/TMP/Murecho-Black SDF.asset");
            CreateFontAsset("Assets/UI/Fonts/SairaCondensed-Black.ttf", "Assets/UI/Fonts/TMP/SairaCondensed-Black SDF.asset");
        }

        private static void CreateFontAsset(string fontPath, string fontAssetPath)
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath) != null)
            {
                return;
            }

            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (sourceFont == null)
            {
                Debug.LogWarning($"[Phase1Bootstrapper] Source font missing: {fontPath}");
                return;
            }

            try
            {
                var fontAsset = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    90,
                    9,
                    GlyphRenderMode.SDFAA,
                    1024,
                    1024,
                    AtlasPopulationMode.Dynamic,
                    true);
                fontAsset.name = Path.GetFileNameWithoutExtension(fontAssetPath);
                AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Phase1Bootstrapper] Could not generate TMP font asset for {fontPath}: {exception.Message}");
            }
        }

        private static void AssignDesignFonts(DesignTokens designTokens)
        {
            designTokens.fontDefault = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/TMP/Murecho-SemiBold SDF.asset");
            designTokens.fontHeading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/TMP/Murecho-Bold SDF.asset");
            designTokens.fontEmphasis = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/TMP/Murecho-Black SDF.asset");
            designTokens.fontDisplay = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/TMP/SairaCondensed-Black SDF.asset");
            EditorUtility.SetDirty(designTokens);
        }

        private static void ConfigureDotweenDefaults()
        {
            var dotweenSettings = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Resources/DOTweenSettings.asset");
            if (dotweenSettings == null)
            {
                Debug.LogWarning("[Phase1Bootstrapper] DOTweenSettings.asset is missing. Open Tools/Demigiant/DOTween Utility Panel if DOTween needs setup.");
                return;
            }

            var serializedSettings = new SerializedObject(dotweenSettings);
            SetBool(serializedSettings, "useSafeMode", true);
            SetInt(serializedSettings, "logBehaviour", 1);
            SetInt(serializedSettings, "defaultEaseType", (int)DG.Tweening.Ease.OutCubic);
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(dotweenSettings);
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void ConfigureIosPlayerSettings()
        {
            try
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Phase1Bootstrapper] Could not switch active build target to iOS: {exception.Message}");
            }

            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            SetArchitectureArm64();

            PlayerSettings.iOS.targetOSVersionString = "16.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
        }

        private static void SetArchitectureArm64()
        {
            var methods = typeof(PlayerSettings).GetMethods()
                .Where(method => method.Name == "SetArchitecture" && method.GetParameters().Length == 2);

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                try
                {
                    if (parameters[0].ParameterType == typeof(UnityEditor.Build.NamedBuildTarget))
                    {
                        method.Invoke(null, new object[] { UnityEditor.Build.NamedBuildTarget.iOS, 1 });
                        return;
                    }

                    if (parameters[0].ParameterType == typeof(BuildTargetGroup))
                    {
                        method.Invoke(null, new object[] { BuildTargetGroup.iOS, 1 });
                        return;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[Phase1Bootstrapper] Could not set iOS architecture via {method}: {exception.Message}");
                }
            }
        }

        private static void ConfigureUrp2D()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<Renderer2DData>(RendererAssetPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(rendererData, RendererAssetPath);
            }

            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipelineAsset == null)
            {
                pipelineAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
            }

            var serializedPipeline = new SerializedObject(pipelineAsset);
            var rendererDataList = serializedPipeline.FindProperty("m_RendererDataList");
            if (rendererDataList != null)
            {
                rendererDataList.arraySize = 1;
                rendererDataList.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
            }

            var defaultRendererIndex = serializedPipeline.FindProperty("m_DefaultRendererIndex");
            if (defaultRendererIndex != null)
            {
                defaultRendererIndex.intValue = 0;
            }

            serializedPipeline.ApplyModifiedPropertiesWithoutUndo();
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;
            EditorUtility.SetDirty(rendererData);
            EditorUtility.SetDirty(pipelineAsset);
        }

        private static void CreateBootScene(DesignTokens designTokens)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = designTokens.bgMain;
            camera.orthographic = true;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var lightObject = new GameObject("Directional Light");
            lightObject.AddComponent<Light>().type = LightType.Directional;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var startupObject = new GameObject("[Startup]");
            var startup = startupObject.AddComponent<Startup>();
            var startupSerialized = new SerializedObject(startup);
            startupSerialized.FindProperty("designTokens").objectReferenceValue = designTokens;
            startupSerialized.ApplyModifiedPropertiesWithoutUndo();

            var canvasObject = new GameObject("Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170f, 2532f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var backgroundObject = new GameObject("Blank Background");
            backgroundObject.transform.SetParent(canvasObject.transform, false);
            var background = backgroundObject.AddComponent<Image>();
            background.color = designTokens.bgMain;
            var rectTransform = background.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            EditorSceneManager.SaveScene(scene, BootScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BootScenePath, true) };
        }
    }
}
#endif
