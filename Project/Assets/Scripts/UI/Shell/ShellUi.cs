using Crashmania.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Shell
{
    internal static class ShellUi
    {
        public static GameObject CreateCanvasRoot(string name, int sortOrder)
        {
            var root = new GameObject(name);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170f, 2532f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return root;
        }

        public static GameObject CreatePanel(string name, Transform parent, Color color)
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

        public static TMP_Text CreateText(string name, Transform parent, string text, DesignTokens tokens, float size, FontStyles style)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            var label = textObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            if (TryGetFont(tokens, out var font))
            {
                label.font = font;
            }

            label.fontSize = size;
            label.fontStyle = style;
            label.color = tokens != null ? tokens.textPrimary : Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return label;
        }

        private static bool TryGetFont(DesignTokens tokens, out TMP_FontAsset font)
        {
            font = tokens != null ? tokens.fontDefault : null;
            if (font == null)
            {
                return false;
            }

            try
            {
                return font.atlasTexture != null;
            }
            catch
            {
                font = null;
                return false;
            }
        }
    }
}
