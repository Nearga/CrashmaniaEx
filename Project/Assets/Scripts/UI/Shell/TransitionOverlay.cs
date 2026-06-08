using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Crashmania.UI.Shell
{
    public sealed class TransitionOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        private Tween fadeTween;

        public static TransitionOverlay Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Initialize();
        }

        private void OnDestroy()
        {
            fadeTween?.Kill();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public UniTask FadeIn(float duration)
        {
            return FadeTo(1f, duration, blocksRaycasts: true);
        }

        public UniTask FadeOut(float duration)
        {
            return FadeTo(0f, duration, blocksRaycasts: false);
        }

        private UniTask FadeTo(float alpha, float duration, bool blocksRaycasts)
        {
            Initialize();
            fadeTween?.Kill();
            fadeCanvasGroup.blocksRaycasts = true;

            var completion = new UniTaskCompletionSource();
            fadeTween = fadeCanvasGroup
                .DOFade(alpha, duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    fadeCanvasGroup.blocksRaycasts = blocksRaycasts;
                    completion.TrySetResult();
                });

            return completion.Task;
        }

        private void Initialize()
        {
            if (fadeCanvasGroup == null)
            {
                fadeCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
            }

            if (fadeCanvasGroup == null)
            {
                Debug.LogError("[TransitionOverlay] Missing fade CanvasGroup.");
                return;
            }

            fadeCanvasGroup.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
    }
}
