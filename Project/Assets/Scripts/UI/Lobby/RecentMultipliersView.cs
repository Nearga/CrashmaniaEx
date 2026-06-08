using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Crashmania.UI.Lobby
{
    /// <summary>
    /// Drives the "RECENT MULTIPLIERS" bar. The label is fixed on the left; the ticker
    /// area scrolls a looping list of multiplier values from right to left using DOTween.
    /// </summary>
    public sealed class RecentMultipliersView : MonoBehaviour
    {
        [SerializeField] private TMP_Text tickerText;

        // Scroll speed in pixels per second (reference app feels ~80 px/s at 1170px wide)
        [SerializeField] private float scrollSpeed = 90f;

        private RectTransform _tickerRT;
        private RectTransform _maskRT;
        private Tweener _scrollTween;

        // Mock data for initial render before real data arrives
        private static readonly float[] DefaultMultipliers =
            { 1.25f, 1.14f, 1.29f, 1.11f, 1.3f, 2.8f, 1.19f, 1.8f, 1.10f, 1.07f, 3.2f, 1.45f };

        private void Awake()
        {
            if (tickerText != null)
                _tickerRT = tickerText.rectTransform;

            _maskRT = tickerText != null
                ? tickerText.transform.parent?.GetComponent<RectTransform>()
                : null;
        }

        private void Start()
        {
            Bind(DefaultMultipliers);
        }

        private void OnDestroy()
        {
            _scrollTween?.Kill();
        }

        /// <summary>Populate ticker with provided multiplier values and start scrolling.</summary>
        public void Bind(IReadOnlyList<float> multipliers)
        {
            if (tickerText == null) return;

            tickerText.text = BuildRichText(multipliers);

            // Wait a frame for TMP to compute the preferred width, then start scrolling
            StartCoroutine(StartScrollingNextFrame());
        }

        private IEnumerator StartScrollingNextFrame()
        {
            yield return null; // wait one frame for TMP layout
            yield return null; // one more to be safe

            Canvas.ForceUpdateCanvases();

            _scrollTween?.Kill();

            if (_tickerRT == null || _maskRT == null) yield break;

            float textWidth = tickerText.preferredWidth;
            float maskWidth = _maskRT.rect.width;

            if (textWidth <= 0f || maskWidth <= 0f) yield break;

            // Start just off the right edge of the mask
            _tickerRT.anchoredPosition = new Vector2(maskWidth, 0f);

            // Scroll until the text has scrolled completely off the left edge,
            // then loop by resetting to start
            float totalTravel = maskWidth + textWidth;
            float duration = totalTravel / Mathf.Max(1f, scrollSpeed);

            _scrollTween = _tickerRT
                .DOAnchorPosX(-textWidth, duration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .OnStepComplete(() =>
                {
                    // Reposition to start of loop instantly
                    _tickerRT.anchoredPosition = new Vector2(maskWidth, 0f);
                });
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static string BuildRichText(IReadOnlyList<float> multipliers)
        {
            var sb = new System.Text.StringBuilder();
            const string sepColor = "#555e72";

            for (int i = 0; i < multipliers.Count; i++)
            {
                if (i > 0)
                    sb.Append($"  <color={sepColor}>|</color>  ");

                float v = multipliers[i];
                string col = MultiplierColor(v);
                sb.Append($"<color={col}>{v:0.##}x</color>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Reference color logic observed from screenshots:
        ///   green  (#11D950) for low multipliers (≤ 2x)
        ///   red    (#E93628) for medium (2x–5x)
        ///   yellow (#FFD700) for high (> 5x)
        /// </summary>
        private static string MultiplierColor(float v)
        {
            if (v > 5f) return "#FFD700";
            if (v > 2f) return "#E93628";
            return "#11D950";
        }
    }
}
