using Crashmania.Config;
using Crashmania.Models;
using Crashmania.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Shell
{
    public sealed class HeaderView : MonoBehaviour
    {
        private AccumulateToBalance ccBalance;
        private AccumulateToBalance scBalance;

        public static HeaderView Create(DesignTokens tokens, AppConfig config)
        {
            var root = ShellUi.CreateCanvasRoot("[HeaderOverlay]", 100);
            DontDestroyOnLoad(root);
            var safeArea = ShellUi.CreatePanel("Safe Area", root.transform, Color.clear);
            safeArea.AddComponent<SafeAreaPanel>();

            var bar = ShellUi.CreatePanel("Header Bar", safeArea.transform, tokens != null ? tokens.bgHeader : new Color(0.282f, 0.325f, 0.392f));
            var barRect = bar.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = Vector2.one;
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.sizeDelta = new Vector2(0f, 120f);
            barRect.anchoredPosition = Vector2.zero;

            var layout = bar.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 18, 18);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            var logo = ShellUi.CreateText("Logo", bar.transform, "CRASHMANIA", tokens, 34, FontStyles.Bold);
            logo.color = tokens != null ? tokens.accentYellow : Color.yellow;
            logo.alignment = TextAlignmentOptions.MidlineLeft;
            logo.gameObject.AddComponent<LayoutElement>().preferredWidth = 520f;

            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(bar.transform, false);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var view = root.AddComponent<HeaderView>();
            view.ccBalance = view.CreateBalanceWidget(bar.transform, "CC", tokens, "N0");
            view.scBalance = view.CreateBalanceWidget(bar.transform, "SC", tokens, "N2");
            view.SetBalances(
                config != null ? config.startingBalanceCC : 0,
                config != null ? config.startingBalanceSC : 0,
                animate: false);
            return view;
        }

        public void SetBalances(PlayerProfile profile, bool animate)
        {
            if (profile == null)
            {
                return;
            }

            SetBalances(profile.BalanceCC, profile.BalanceSC, animate);
        }

        public void SetBalances(double cc, double sc, bool animate)
        {
            ccBalance.SetValue(cc, animate);
            scBalance.SetValue(sc, animate);
        }

        public void SetVisibleForScene(string sceneName)
        {
            gameObject.SetActive(sceneName != "Game");
        }

        private AccumulateToBalance CreateBalanceWidget(Transform parent, string prefix, DesignTokens tokens, string format)
        {
            var widget = ShellUi.CreatePanel($"{prefix} Balance", parent, tokens != null ? tokens.bgCard : Color.gray);
            widget.AddComponent<LayoutElement>().preferredWidth = prefix == "CC" ? 230f : 170f;

            var layout = widget.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 10, 10);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;

            var label = ShellUi.CreateText($"{prefix} Prefix", widget.transform, prefix, tokens, 24, FontStyles.Bold);
            label.color = prefix == "SC" && tokens != null ? tokens.accentGreen : (tokens != null ? tokens.accentYellow : Color.yellow);

            var value = ShellUi.CreateText($"{prefix} Value", widget.transform, string.Empty, tokens, 24, FontStyles.Bold);
            value.alignment = TextAlignmentOptions.MidlineRight;
            value.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var accumulator = value.gameObject.AddComponent<AccumulateToBalance>();
            accumulator.SetFormat(format);
            return accumulator;
        }
    }
}
