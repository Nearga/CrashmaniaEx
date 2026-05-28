using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Shell
{
    public sealed class ModalView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Button overlayButton;

        private readonly Queue<GameObject> queuedPrefabs = new();
        private readonly Stack<GameObject> modalStack = new();

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (panel == null) panel = transform.Find("Modal Overlay/Modal Panel")?.GetComponent<RectTransform>();
            if (overlayButton == null) overlayButton = transform.Find("Modal Overlay")?.GetComponent<Button>();

            if (overlayButton != null)
            {
                overlayButton.onClick.RemoveAllListeners();
                overlayButton.onClick.AddListener(Hide);
            }
            
            HideImmediate();
        }

        public void Show(object payload = null)
        {
            if (payload is GameObject prefab)
            {
                ShowPrefab(prefab);
                return;
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                canvasGroup.DOFade(1f, 0.25f);
            }
            if (panel != null)
            {
                panel.localScale = Vector3.one * 0.8f;
                panel.DOScale(1f, 0.25f).SetEase(Ease.OutCubic);
            }
        }

        public void Hide()
        {
            if (modalStack.Count > 0)
            {
                Destroy(modalStack.Pop());
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                canvasGroup.DOFade(0f, 0.2f).OnComplete(ShowNextQueuedPrefab);
            }
            if (panel != null)
            {
                panel.DOScale(0.8f, 0.2f).SetEase(Ease.OutCubic);
            }
        }

        public void Enqueue(GameObject modalPrefab)
        {
            if (modalPrefab == null)
            {
                return;
            }

            queuedPrefabs.Enqueue(modalPrefab);
            if (modalStack.Count == 0 && (canvasGroup == null || canvasGroup.alpha <= 0f))
            {
                ShowNextQueuedPrefab();
            }
        }

        private void ShowPrefab(GameObject modalPrefab)
        {
            if (modalStack.Count > 0)
            {
                queuedPrefabs.Enqueue(modalPrefab);
                return;
            }

            if (panel != null)
            {
                for (var index = panel.childCount - 1; index >= 0; index--)
                {
                    Destroy(panel.GetChild(index).gameObject);
                }

                var instance = Instantiate(modalPrefab, panel);
                modalStack.Push(instance);
            }
            Show();
        }

        private void ShowNextQueuedPrefab()
        {
            if (queuedPrefabs.Count > 0)
            {
                ShowPrefab(queuedPrefabs.Dequeue());
            }
        }

        private void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            if (panel != null)
            {
                panel.localScale = Vector3.one * 0.8f;
            }
        }
    }
}
