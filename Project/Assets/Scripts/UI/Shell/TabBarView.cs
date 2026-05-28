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

        public void Initialize(DesignTokens tokens)
        {
            this.tokens = tokens;
        }

        private void Awake()
        {
            BindTab("Lobby", "HOME Tab");
            BindTab("Store", "STORE Tab");
            BindTab("Gifts", "GIFTS Tab");
            BindTab("Account", "ACCOUNT Tab");
        }

        private void BindTab(string sceneName, string pathName)
        {
            var tabTransform = transform.Find($"Safe Area/Tab Bar/{pathName}");
            if (tabTransform == null) return;

            var button = tabTransform.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => TabSelected?.Invoke(sceneName));
            }

            var iconImage = tabTransform.Find("Icon")?.GetComponent<Image>();
            var labelText = tabTransform.Find("Label")?.GetComponent<TMP_Text>();

            if (iconImage != null && labelText != null)
            {
                tabs[sceneName] = new TabButton(tabTransform, iconImage, labelText);
            }
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
