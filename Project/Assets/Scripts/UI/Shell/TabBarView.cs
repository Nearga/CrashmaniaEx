using System;
using System.Collections.Generic;
using Crashmania.Config;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Shell
{
    public sealed class TabBarView : MonoBehaviour
    {
        private readonly Dictionary<string, TabButton> tabs = new();
        private DesignTokens tokens;

        public event Action<string> TabSelected;

        public static TabBarView Create(DesignTokens tokens)
        {
            var root = ShellUi.CreateCanvasRoot("[TabBar]", 100);
            DontDestroyOnLoad(root);
            var safeArea = ShellUi.CreatePanel("Safe Area", root.transform, Color.clear);
            safeArea.AddComponent<Crashmania.UI.Components.SafeAreaPanel>();

            var bar = ShellUi.CreatePanel("Tab Bar", safeArea.transform, tokens != null ? tokens.bgFooter : new Color(0.1f, 0.1f, 0.12f));
            var rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, 150f);
            rect.anchoredPosition = Vector2.zero;

            var layout = bar.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var view = root.AddComponent<TabBarView>();
            view.tokens = tokens;
            view.AddTab(bar.transform, "Lobby", "HOME");
            view.AddTab(bar.transform, "Store", "STORE");
            view.AddTab(bar.transform, "Gifts", "GIFTS");
            view.AddTab(bar.transform, "Account", "ACCOUNT");
            view.Highlight("Lobby");
            return view;
        }

        public void Highlight(string sceneName)
        {
            foreach (var tab in tabs)
            {
                tab.Value.SetActive(tab.Key == sceneName, tokens);
            }
        }

        public void SetVisibleForScene(string sceneName)
        {
            gameObject.SetActive(IsShellScene(sceneName));
        }

        private static bool IsShellScene(string sceneName)
        {
            return sceneName == "Lobby" || sceneName == "Store" || sceneName == "Gifts" || sceneName == "Account";
        }

        private void AddTab(Transform parent, string sceneName, string label)
        {
            var buttonObject = ShellUi.CreatePanel($"{label} Tab", parent, Color.clear);
            buttonObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var button = buttonObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => TabSelected?.Invoke(sceneName));

            var layout = buttonObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 22, 18);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandHeight = false;

            var iconObject = ShellUi.CreatePanel("Icon", buttonObject.transform, tokens != null ? tokens.textSecondary : Color.gray);
            var iconImage = iconObject.GetComponent<Image>();
            iconImage.raycastTarget = false;
            iconObject.AddComponent<LayoutElement>().preferredHeight = 42f;
            iconObject.GetComponent<LayoutElement>().preferredWidth = 42f;

            var labelText = ShellUi.CreateText("Label", buttonObject.transform, label, tokens, 24, FontStyles.Bold);
            labelText.gameObject.AddComponent<LayoutElement>().preferredHeight = 34f;
            tabs[sceneName] = new TabButton(buttonObject.transform, iconImage, labelText);
        }

        private readonly struct TabButton
        {
            private readonly Transform root;
            private readonly Image icon;
            private readonly TMP_Text label;

            public TabButton(Transform root, Image icon, TMP_Text label)
            {
                this.root = root;
                this.icon = icon;
                this.label = label;
            }

            public void SetActive(bool active, DesignTokens tokens)
            {
                var activeColor = tokens != null ? tokens.brandPurple : Color.magenta;
                var inactiveColor = tokens != null ? tokens.textSecondary : Color.gray;
                icon.DOColor(active ? activeColor : inactiveColor, 0.15f);
                label.DOColor(active ? Color.white : inactiveColor, 0.15f);
                root.DOScale(active ? 1.05f : 1f, 0.15f);
            }
        }
    }
}
