#if UNITY_EDITOR
using System;
using System.IO;
using Crashmania.UI.Components;
using Crashmania.UI.Modals;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase35ModalVerifier
    {
        [MenuItem("Crashmania/Verify Phase 3.5 Login Modals")]
        public static void Run()
        {
            VerifyLoginModal();
            VerifySignupModal();
            VerifySignupPrePopup();
            VerifyPureMvcBoundaries();
            Debug.Log("[Phase35ModalVerifier] Phase 3.5 login modal verification completed.");
        }

        private static void VerifyLoginModal()
        {
            var prefab = Load("Assets/Resources/UI/Modals/LoginModal.prefab");
            Require<LoginModalView>(prefab, "LoginModalView");
            RequirePath(prefab, "Panel/Header/Title");
            RequirePath(prefab, "Panel/FacebookButton");
            RequirePath(prefab, "Panel/GoogleButton");
            RequirePath(prefab, "Panel/Or");
            RequirePath(prefab, "Panel/Email");
            RequirePath(prefab, "Panel/LoginButton");
            RequirePath(prefab, "CloseBtn");
            RequireButton(prefab, "Panel/LoginButton");
            RequireButton(prefab, "CloseBtn");
            RequireSkew(prefab, "Panel/FacebookButton");
            RequireSkew(prefab, "Panel/GoogleButton");
            RequireSkew(prefab, "Panel/LoginButton");
        }

        private static void VerifySignupModal()
        {
            var prefab = Load("Assets/Resources/UI/Modals/SignupModal.prefab");
            Require<SignupModalView>(prefab, "SignupModalView");
            RequirePath(prefab, "Panel/Header/Title");
            RequirePath(prefab, "Panel/FacebookButton");
            RequirePath(prefab, "Panel/GoogleButton");
            RequirePath(prefab, "Panel/Or");
            RequirePath(prefab, "Panel/EmailButton");
            RequirePath(prefab, "Panel/AlreadyText");
            RequirePath(prefab, "CloseBtn");
            RequireButton(prefab, "Panel/EmailButton");
            RequireButton(prefab, "CloseBtn");
            RequireSkew(prefab, "Panel/FacebookButton");
            RequireSkew(prefab, "Panel/GoogleButton");
            RequireSkew(prefab, "Panel/EmailButton");
        }

        private static void VerifySignupPrePopup()
        {
            var prefab = Load("Assets/Resources/UI/Modals/SignupPrePopupModal.prefab");
            Require<SignupPrePopupModalView>(prefab, "SignupPrePopupModalView");
            RequirePath(prefab, "Panel/Header/Title");
            RequirePath(prefab, "Panel/MessageTop");
            RequirePath(prefab, "Panel/MessageBottom");
            RequirePath(prefab, "Panel/AcceptButton");
            RequirePath(prefab, "CloseBtn");
            RequireButton(prefab, "Panel/AcceptButton");
            RequireButton(prefab, "CloseBtn");
            RequireSkew(prefab, "Panel/AcceptButton");
        }

        private static GameObject Load(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Missing modal prefab: {path}");
            }

            return prefab;
        }

        private static void RequirePath(GameObject prefab, string path)
        {
            if (prefab.transform.Find(path) == null)
            {
                throw new InvalidOperationException($"{prefab.name} is missing {path}");
            }
        }

        private static void RequireButton(GameObject prefab, string path)
        {
            var transform = prefab.transform.Find(path);
            if (transform == null || transform.GetComponent<Button>() == null)
            {
                throw new InvalidOperationException($"{prefab.name}/{path} is missing Button.");
            }
        }

        private static void RequireSkew(GameObject prefab, string path)
        {
            var transform = prefab.transform.Find(path);
            if (transform == null || transform.GetComponent<SkewRect>() == null)
            {
                throw new InvalidOperationException($"{prefab.name}/{path} is missing SkewRect.");
            }
        }

        private static void Require<T>(GameObject prefab, string componentName) where T : Component
        {
            if (prefab.GetComponent<T>() == null)
            {
                throw new InvalidOperationException($"{prefab.name} is missing {componentName}.");
            }
        }

        private static void VerifyPureMvcBoundaries()
        {
            var uiViewSources = new[]
            {
                "Assets/Scripts/UI/Login/LoginView.cs",
                "Assets/Scripts/UI/Modals/LoginModalView.cs",
                "Assets/Scripts/UI/Modals/SignupModalView.cs",
                "Assets/Scripts/UI/Modals/SignupPrePopupModalView.cs"
            };

            foreach (var assetPath in uiViewSources)
            {
                var fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
                var source = File.ReadAllText(fullPath);
                if (source.Contains("LobbyFacade.GetInstance", StringComparison.Ordinal) ||
                    source.Contains("SendNotification", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{assetPath} must stay view-only and communicate through events, not PureMVC facade calls.");
                }
            }
        }
    }
}
#endif
