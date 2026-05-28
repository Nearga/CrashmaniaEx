#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using Crashmania.Config;
using Crashmania.Core;
using Crashmania.Services;
using Crashmania.UI.Components;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase1Verifier
    {
        [MenuItem("Crashmania/Verify Phase 1")]
        public static void Run()
        {
            VerifyPackages();
            VerifyPlayerSettings();
            VerifyDependencyContainer();
            VerifyAssets();
            VerifyBootScene();
            Debug.Log("[Phase1Verifier] Phase 1 verification completed.");
        }

        private static void VerifyDependencyContainer()
        {
            var container = DependencyContainer.Instance;
            container.Clear();
            container.Register<string>("phase-one");

            if (container.Resolve<string>() != "phase-one")
            {
                throw new InvalidOperationException("DependencyContainer did not resolve the registered string.");
            }

            var target = new InjectionTarget();
            container.Inject(target);

            if (target.Value != "phase-one")
            {
                throw new InvalidOperationException("DependencyContainer did not inject the registered string.");
            }
        }

        private static void VerifyAssets()
        {
            var config = AssetDatabase.LoadAssetAtPath<AppConfig>("Assets/Resources/AppConfig.asset");
            if (config == null)
            {
                throw new InvalidOperationException("AppConfig.asset is missing.");
            }

            var backend = new MockBackendService(config);
            if (backend == null)
            {
                throw new InvalidOperationException("MockBackendService could not be constructed.");
            }

            var designTokens = AssetDatabase.LoadAssetAtPath<DesignTokens>("Assets/ScriptableObjects/DesignTokens.asset");
            if (designTokens == null)
            {
                throw new InvalidOperationException("DesignTokens.asset is missing.");
            }

            AssertAsset(designTokens.fontDefault, "DesignTokens.fontDefault");
            AssertAsset(designTokens.fontHeading, "DesignTokens.fontHeading");
            AssertAsset(designTokens.fontEmphasis, "DesignTokens.fontEmphasis");
            AssertAsset(designTokens.fontDisplay, "DesignTokens.fontDisplay");

            AssertAsset(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/TMP/Murecho-SemiBold SDF.asset"), "Murecho SemiBold TMP font");
            AssertAsset(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/TMP/Murecho-Bold SDF.asset"), "Murecho Bold TMP font");
            AssertAsset(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/TMP/Murecho-Black SDF.asset"), "Murecho Black TMP font");
            AssertAsset(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/TMP/SairaCondensed-Black SDF.asset"), "Saira Condensed TMP font");

            if (DOTween.Version == null)
            {
                throw new InvalidOperationException("DOTween is not available.");
            }

            if (typeof(DOTweenAnimation) == null)
            {
                throw new InvalidOperationException("DOTweenAnimation type is not available.");
            }

            AssertAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Plugins/Demigiant/DOTweenPro/DOTweenPro.dll"), "DOTweenPro.dll");
            AssertAsset(AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Plugins/Demigiant/DOTweenPro/DOTweenAnimation.cs"), "DOTween Pro DOTweenAnimation.cs");
            AssertAsset(AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Plugins/Demigiant/DOTweenPro/DOTweenProShortcuts.cs"), "DOTween Pro shortcuts");
            AssertAsset(AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Resources/DOTweenSettings.asset"), "DOTweenSettings.asset");

            if (DOTween.defaultEaseType != Ease.OutCubic)
            {
                DOTween.defaultEaseType = Ease.OutCubic;
            }
        }

        private static void VerifyBootScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
            if (!scene.IsValid())
            {
                throw new InvalidOperationException("Boot scene could not be opened.");
            }

            if (GameObject.Find("[Startup]") == null)
            {
                throw new InvalidOperationException("Boot scene is missing [Startup].");
            }

            var startup = GameObject.Find("[Startup]").GetComponent<Startup>();
            if (startup == null)
            {
                throw new InvalidOperationException("[Startup] is missing Startup component.");
            }

            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                throw new InvalidOperationException("Boot scene is missing Canvas.");
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null ||
                scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                scaler.referenceResolution != CanvasResolutionPolicy.ReferenceResolution ||
                Math.Abs(scaler.matchWidthOrHeight - CanvasResolutionPolicy.MatchWidthOrHeight) > 0.001f)
            {
                throw new InvalidOperationException("Boot scene CanvasScaler does not match Phase 1 mobile reference settings.");
            }

            if (GameObject.Find("Main Camera") == null)
            {
                throw new InvalidOperationException("Boot scene is missing Main Camera.");
            }

            if (GameObject.Find("Directional Light") == null)
            {
                throw new InvalidOperationException("Boot scene is missing Directional Light.");
            }

            if (!EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == "Assets/Scenes/Boot.unity"))
            {
                throw new InvalidOperationException("Boot scene is not enabled in Build Settings.");
            }
        }

        private static void VerifyPackages()
        {
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            var manifest = File.ReadAllText(manifestPath);

            foreach (var dependency in new[]
                     {
                         "com.unity.textmeshpro",
                         "com.cysharp.unitask",
                         "com.unity.addressables",
                         "com.unity.nuget.newtonsoft-json"
                     })
            {
                if (!manifest.Contains(dependency))
                {
                    throw new InvalidOperationException($"Package dependency is missing from manifest: {dependency}");
                }
            }

            AssertAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Plugins/PureMVC/PureMVC.DotNET.35.dll"), "PureMVC DLL");
            AssertAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Plugins/Demigiant/DOTweenPro/DOTweenPro.dll"), "DOTween Pro DLL");
        }

        private static void VerifyPlayerSettings()
        {
            if (PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.iOS) != ScriptingImplementation.IL2CPP)
            {
                throw new InvalidOperationException("iOS scripting backend must be IL2CPP.");
            }

            if (PlayerSettings.iOS.targetOSVersionString != "16.0")
            {
                throw new InvalidOperationException("Minimum iOS version must be 16.0.");
            }

            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait ||
                PlayerSettings.allowedAutorotateToLandscapeLeft ||
                PlayerSettings.allowedAutorotateToLandscapeRight ||
                PlayerSettings.allowedAutorotateToPortraitUpsideDown)
            {
                throw new InvalidOperationException("iOS orientation settings must be portrait-only.");
            }

            var pipelineAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (pipelineAsset == null)
            {
                throw new InvalidOperationException("Default render pipeline is not URP.");
            }
        }

        private static void AssertAsset(UnityEngine.Object asset, string name)
        {
            if (asset == null)
            {
                throw new InvalidOperationException($"{name} is missing.");
            }
        }

        private sealed class InjectionTarget
        {
            [Inject] private string value;

            public string Value => value;
        }
    }
}
#endif
