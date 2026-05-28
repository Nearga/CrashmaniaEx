using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Shell
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class TransitionOverlay : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private Tween fadeTween;

        public static TransitionOverlay Instance { get; private set; }

        public static TransitionOverlay Create()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var root = new GameObject("[TransitionOverlay]");
            DontDestroyOnLoad(root);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;
            root.AddComponent<GraphicRaycaster>();

            var overlay = root.AddComponent<TransitionOverlay>();
            var image = root.AddComponent<Image>();
            image.color = Color.black;

            var rectTransform = root.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            overlay.Initialize();
            overlay.canvasGroup.alpha = 0f;
            overlay.canvasGroup.blocksRaycasts = false;
            return overlay;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            canvasGroup.blocksRaycasts = true;

            var completion = new UniTaskCompletionSource();
            fadeTween = canvasGroup
                .DOFade(alpha, duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    canvasGroup.blocksRaycasts = blocksRaycasts;
                    completion.TrySetResult();
                });

            return completion.Task;
        }

        private void Initialize()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
}
