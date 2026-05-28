using System;
using Crashmania.Config;
using Crashmania.Models;
using Crashmania.UI.Components;
using Crashmania.UI.Shell;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Login
{
    public sealed class LoginView : MonoBehaviour
    {
        private TMP_InputField emailInput;
        private TMP_InputField passwordInput;
        private TMP_Text loginLabel;
        private TMP_Text errorLabel;
        private Button loginButton;
        private Button googleButton;

        public event Action<LoginCredentials> SubmitRequested;
        public event Action SignUpSelected;

        public static LoginView Create(DesignTokens tokens)
        {
            var existing = FindAnyObjectByType<LoginView>();
            if (existing != null)
            {
                return existing;
            }

            var canvas = FindAnyObjectByType<Canvas>() ?? CreateCanvas();
            var root = ShellUi.CreatePanel("LoginView", canvas.transform, Color.clear);
            var view = root.AddComponent<LoginView>();
            view.Build(tokens);
            return view;
        }

        public void SetLoading(bool isLoading)
        {
            loginButton.interactable = !isLoading;
            googleButton.interactable = !isLoading;
            loginLabel.text = isLoading ? "LOGGING IN..." : "LOGIN";
            ShowError(null);
        }

        public void ShowError(string message)
        {
            errorLabel.text = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170f, 2532f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private void Build(DesignTokens tokens)
        {
            var rootRect = GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var background = GetComponent<Image>();
            background.color = Color.white;
            background.raycastTarget = false;
            var gradient = gameObject.AddComponent<GradientImage>();
            gradient.SetColors(
                tokens != null ? tokens.bgMain : new Color(0.157f, 0.169f, 0.220f),
                new Color(0.04f, 0.045f, 0.07f));

            var content = new GameObject("Content");
            content.transform.SetParent(transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(820f, 1260f);
            contentRect.anchoredPosition = new Vector2(0f, 80f);

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 28f;

            var logo = ShellUi.CreateText("Logo", content.transform, "CRASHMANIA", tokens, 86f, FontStyles.Bold);
            logo.color = tokens != null ? tokens.accentYellow : Color.yellow;
            logo.gameObject.AddComponent<LayoutElement>().preferredHeight = 220f;

            emailInput = CreateInput(content.transform, "Email", "Email", TMP_InputField.ContentType.EmailAddress, tokens);
            passwordInput = CreateInput(content.transform, "Password", "Password", TMP_InputField.ContentType.Password, tokens);

            loginButton = CreateButton(content.transform, "Login Button", "LOGIN", tokens, filled: true, out loginLabel);
            loginButton.onClick.AddListener(SubmitEmail);

            googleButton = CreateButton(content.transform, "Google Button", "Continue with Google", tokens, filled: false, out _);
            googleButton.onClick.AddListener(SubmitGoogle);

            var signUp = ShellUi.CreateText("Sign Up", content.transform, "Sign Up", tokens, 30f, FontStyles.Bold);
            signUp.color = tokens != null ? tokens.ctaBlueTop : new Color(0.157f, 0.439f, 1f);
            signUp.gameObject.AddComponent<LayoutElement>().preferredHeight = 64f;
            var signUpButton = signUp.gameObject.AddComponent<Button>();
            signUpButton.transition = Selectable.Transition.None;
            signUpButton.onClick.AddListener(() => SignUpSelected?.Invoke());

            errorLabel = ShellUi.CreateText("Error", content.transform, string.Empty, tokens, 26f, FontStyles.Bold);
            errorLabel.color = tokens != null ? tokens.errorRed : Color.red;
            errorLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 72f;
        }

        private TMP_InputField CreateInput(Transform parent, string name, string placeholderText, TMP_InputField.ContentType contentType, DesignTokens tokens)
        {
            var container = ShellUi.CreatePanel(name, parent, Color.clear);
            container.AddComponent<LayoutElement>().preferredHeight = 112f;

            var input = container.AddComponent<TMP_InputField>();
            input.contentType = contentType;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 120;

            var text = ShellUi.CreateText("Text", container.transform, string.Empty, tokens, 34f, FontStyles.Normal);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.color = tokens != null ? tokens.textPrimary : Color.white;
            text.raycastTarget = true;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(0f, 14f);
            textRect.offsetMax = new Vector2(0f, -8f);

            var placeholder = ShellUi.CreateText("Placeholder", container.transform, placeholderText, tokens, 34f, FontStyles.Normal);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.color = tokens != null ? tokens.textSecondary : Color.gray;
            var placeholderRect = placeholder.GetComponent<RectTransform>();
            placeholderRect.anchorMin = textRect.anchorMin;
            placeholderRect.anchorMax = textRect.anchorMax;
            placeholderRect.offsetMin = textRect.offsetMin;
            placeholderRect.offsetMax = textRect.offsetMax;

            var underline = ShellUi.CreatePanel("Underline", container.transform, tokens != null ? tokens.textSecondary : Color.gray);
            var underlineRect = underline.GetComponent<RectTransform>();
            underlineRect.anchorMin = Vector2.zero;
            underlineRect.anchorMax = new Vector2(1f, 0f);
            underlineRect.sizeDelta = new Vector2(0f, 4f);
            underlineRect.anchoredPosition = Vector2.zero;

            input.textViewport = textRect;
            input.textComponent = text;
            input.placeholder = placeholder;
            return input;
        }

        private Button CreateButton(Transform parent, string name, string label, DesignTokens tokens, bool filled, out TMP_Text labelText)
        {
            var buttonObject = ShellUi.CreatePanel(name, parent, filled ? Color.white : Color.clear);
            buttonObject.AddComponent<LayoutElement>().preferredHeight = 112f;
            var image = buttonObject.GetComponent<Image>();
            image.raycastTarget = true;

            if (filled)
            {
                var gradient = buttonObject.AddComponent<GradientImage>();
                gradient.SetColors(
                    tokens != null ? tokens.ctaBlueTop : new Color(0.157f, 0.439f, 1f),
                    tokens != null ? tokens.ctaBlueEnd : new Color(0.478f, 0.22f, 0.988f));
                var skew = buttonObject.AddComponent<SkewRect>();
                skew.Angle = -5f;
            }
            else
            {
                image.color = new Color(1f, 1f, 1f, 0.04f);
                var outline = buttonObject.AddComponent<Outline>();
                outline.effectColor = tokens != null ? tokens.textSecondary : Color.gray;
                outline.effectDistance = new Vector2(2f, -2f);
            }

            var button = buttonObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            labelText = ShellUi.CreateText("Label", buttonObject.transform, label, tokens, 32f, FontStyles.Bold);
            labelText.color = filled ? Color.white : (tokens != null ? tokens.textPrimary : Color.white);
            return button;
        }

        private void SubmitEmail()
        {
            SubmitRequested?.Invoke(new LoginCredentials
            {
                Provider = LoginProvider.Email,
                Email = emailInput.text,
                Password = passwordInput.text
            });
        }

        private void SubmitGoogle()
        {
            SubmitRequested?.Invoke(new LoginCredentials
            {
                Provider = LoginProvider.Google,
                GoogleIdToken = "mock-google-id-token"
            });
        }
    }
}
