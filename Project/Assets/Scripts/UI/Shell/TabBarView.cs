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
        [Header("Lobby Tab")]
        [SerializeField] private Button lobbyButton;
        [SerializeField] private Image lobbyIcon;
        [SerializeField] private TMP_Text lobbyLabel;

        [Header("Store Tab")]
        [SerializeField] private Button storeButton;
        [SerializeField] private Image storeIcon;
        [SerializeField] private TMP_Text storeLabel;

        [Header("Gifts Tab")]
        [SerializeField] private Button giftsButton;
        [SerializeField] private Image giftsIcon;
        [SerializeField] private TMP_Text giftsLabel;

        [Header("Account Tab")]
        [SerializeField] private Button accountButton;
        [SerializeField] private Image accountIcon;
        [SerializeField] private TMP_Text accountLabel;

        private readonly Dictionary<string, TabButton> tabs = new();
        private DesignTokens tokens;

        public event Action<string> TabSelected;

        public void Initialize(DesignTokens tokens)
        {
            this.tokens = tokens;
        }

        private void Awake()
        {
            BindTab("Lobby", lobbyButton, lobbyIcon, lobbyLabel);
            BindTab("Store", storeButton, storeIcon, storeLabel);
            BindTab("Gifts", giftsButton, giftsIcon, giftsLabel);
            BindTab("Account", accountButton, accountIcon, accountLabel);
        }

        private void BindTab(string sceneName, Button button, Image icon, TMP_Text label)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => TabSelected?.Invoke(sceneName));
            }

            if (button != null && icon != null && label != null)
            {
                tabs[sceneName] = new TabButton(button.transform, icon, label);
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
