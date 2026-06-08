using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Crashmania.UI.Components
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class AccumulateToBalance : MonoBehaviour
    {
        [SerializeField] private string format = "N0";
        [SerializeField] private float defaultAnimationDuration = 0.5f;

        private TMP_Text label;
        private double currentValue;
        private Tween tween;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
        }

        private void OnDestroy()
        {
            tween?.Kill();
        }

        public void SetValue(double value, bool animate)
        {
            SetValue(value, animate, defaultAnimationDuration);
        }

        public void SetValue(double value, bool animate, float duration)
        {
            if (label == null)
            {
                label = GetComponent<TMP_Text>();
            }

            tween?.Kill();

            if (!animate)
            {
                currentValue = value;
                label.text = Format(value);
                return;
            }

            var start = currentValue;
            tween = DOTween.To(() => start, nextValue =>
                {
                    start = nextValue;
                    currentValue = nextValue;
                    label.text = Format(currentValue);
                }, value, Mathf.Max(0.01f, duration))
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    currentValue = value;
                    label.text = Format(value);
                });
        }

        public void SetFormat(string valueFormat)
        {
            format = valueFormat;
            SetValue(currentValue, animate: false);
        }

        private string Format(double value)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
