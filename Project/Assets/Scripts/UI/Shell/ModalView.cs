using System.Collections.Generic;
using Crashmania.Config;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Shell
{
    public sealed class ModalView : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private RectTransform panel;
        private readonly Queue<GameObject> queuedPrefabs = new();
        private readonly Stack<GameObject> modalStack = new();

        public static ModalView Create(DesignTokens tokens)
        {
            var root = ShellUi.CreateCanvasRoot("[ModalManager]", 200);
            DontDestroyOnLoad(root);
            var view = root.AddComponent<ModalView>();

            var overlay = ShellUi.CreatePanel("Modal Overlay", root.transform, new Color(0f, 0f, 0f, 0.7f));
            overlay.AddComponent<Button>().onClick.AddListener(view.Hide);

            var modalPanel = ShellUi.CreatePanel("Modal Panel", overlay.transform, tokens != null ? tokens.bgMain : Color.black);
            view.panel = modalPanel.GetComponent<RectTransform>();
            view.panel.anchorMin = new Vector2(0.5f, 0.5f);
            view.panel.anchorMax = new Vector2(0.5f, 0.5f);
            view.panel.pivot = new Vector2(0.5f, 0.5f);
            view.panel.sizeDelta = new Vector2(760f, 420f);

            var label = ShellUi.CreateText("Placeholder", modalPanel.transform, "MODAL", tokens, 36, FontStyles.Bold);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            view.canvasGroup = root.AddComponent<CanvasGroup>();
            view.HideImmediate();
            return view;
        }

        public void Show(object payload = null)
        {
            if (payload is GameObject prefab)
            {
                ShowPrefab(prefab);
                return;
            }

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            panel.localScale = Vector3.one * 0.8f;
            canvasGroup.DOFade(1f, 0.25f);
            panel.DOScale(1f, 0.25f).SetEase(Ease.OutCubic);
        }

        public void Hide()
        {
            if (modalStack.Count > 0)
            {
                Destroy(modalStack.Pop());
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.DOFade(0f, 0.2f).OnComplete(ShowNextQueuedPrefab);
            panel.DOScale(0.8f, 0.2f).SetEase(Ease.OutCubic);
        }

        public void Enqueue(GameObject modalPrefab)
        {
            if (modalPrefab == null)
            {
                return;
            }

            queuedPrefabs.Enqueue(modalPrefab);
            if (modalStack.Count == 0 && canvasGroup.alpha <= 0f)
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

            for (var index = panel.childCount - 1; index >= 0; index--)
            {
                Destroy(panel.GetChild(index).gameObject);
            }

            var instance = Instantiate(modalPrefab, panel);
            modalStack.Push(instance);
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
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            panel.localScale = Vector3.one * 0.8f;
        }
    }
}
