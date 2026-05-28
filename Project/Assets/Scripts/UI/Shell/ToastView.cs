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
            if (panel == null) panel = transform.Find("Safe Area/Toast Panel")?.GetComponent<RectTransform>();
            if (messageText == null) messageText = transform.Find("Safe Area/Toast Panel/Message")?.GetComponent<TMP_Text>();
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
