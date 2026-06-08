using Crashmania.Config;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Shell
{
    public sealed class ToastView : MonoBehaviour
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private TMP_Text messageText;
        private Sequence currentSequence;

        private void Awake()
        {
            // Toast is a passive notification — it must never block input to canvases below it.
            // The Safe Area background image (alpha=0) was intercepting all drags/clicks
            // before they could reach the ScrollRect in the lower-sortOrder LoginScreen canvas.
            foreach (var g in GetComponentsInChildren<Graphic>(true))
                g.raycastTarget = false;
        }

        public void Show(string message)
        {
            messageText.text = message;

            currentSequence?.Kill();
            
            // Reset position if currently animating
            panel.anchoredPosition = new Vector2(0f, 200f);

            currentSequence = DOTween.Sequence();
            currentSequence.Append(panel.DOAnchorPosY(-40f, 0.3f).SetEase(Ease.OutCubic));
            currentSequence.AppendInterval(2f);
            currentSequence.Append(panel.DOAnchorPosY(200f, 0.3f).SetEase(Ease.InCubic));
        }

        private void OnDestroy()
        {
            currentSequence?.Kill();
        }
    }
}
