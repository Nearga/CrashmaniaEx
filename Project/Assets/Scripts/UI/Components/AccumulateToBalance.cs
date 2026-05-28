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
            tween = DOVirtual.Float((float)start, (float)value, 0.5f, nextValue =>
                {
                    currentValue = nextValue;
                    label.text = Format(currentValue);
                })
                .SetEase(Ease.OutCubic);
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
