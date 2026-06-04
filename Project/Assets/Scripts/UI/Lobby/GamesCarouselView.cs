using System;
using System.Collections.Generic;
using Crashmania.Models;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Lobby
{
    public sealed class GamesCarouselView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button viewAllButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private Image leftFade;
        [SerializeField] private Image rightFade;

        private string categoryId;
        #pragma warning disable CS0414
        private bool isSnapping;
#pragma warning restore CS0414
        private float cardWidth = 290f;
        private float snapDuration = 0.3f;

        public event Action<string> ViewAllRequested;
        public event Action<string> GameSelected;

        private void Awake()
        {
            if (viewAllButton != null) viewAllButton.onClick.AddListener(() => ViewAllRequested?.Invoke(categoryId));
            if (previousButton != null) previousButton.onClick.AddListener(() => Nudge(-1));
            if (nextButton != null) nextButton.onClick.AddListener(() => Nudge(1));

            if (scrollRect != null)
            {
                scrollRect.inertia = true;
                scrollRect.decelerationRate = 0.01f;
                scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            }

            UpdateFadeVisibility();
        }

        private void OnDestroy()
        {
            if (viewAllButton != null) viewAllButton.onClick.RemoveAllListeners();
            if (previousButton != null) previousButton.onClick.RemoveAllListeners();
            if (nextButton != null) nextButton.onClick.RemoveAllListeners();

            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            }
        }

        public void Bind(CategoryModel category, GameCardView gameCardPrefab, GameCardView topGameCardPrefab)
        {
            categoryId = category != null ? category.Id : string.Empty;
            if (titleText != null) titleText.text = category != null ? category.Name : string.Empty;
            if (content == null || category == null)
            {
                return;
            }

            Clear(content);
            var prefab = category.Id == "lucky-week" && topGameCardPrefab != null ? topGameCardPrefab : gameCardPrefab;
            var rank = 1;
            foreach (var game in category.Games)
            {
                var card = Instantiate(prefab, content);
                card.Bind(game, category.Id == "lucky-week" ? rank : 0);
                card.Selected += id => GameSelected?.Invoke(id);
                rank++;
            }

            Canvas.ForceUpdateCanvases();
            UpdateFadeVisibility();
        }

        public void BindSearchResults(string title, IReadOnlyList<GameModel> games, GameCardView gameCardPrefab)
        {
            categoryId = "search";
            if (titleText != null) titleText.text = title;
            if (content == null)
            {
                return;
            }

            Clear(content);
            foreach (var game in games)
            {
                var card = Instantiate(gameCardPrefab, content);
                card.Bind(game);
                card.Selected += id => GameSelected?.Invoke(id);
            }

            Canvas.ForceUpdateCanvases();
            UpdateFadeVisibility();
        }

        private static void Clear(Transform target)
        {
            for (var index = target.childCount - 1; index >= 0; index--)
            {
                Destroy(target.GetChild(index).gameObject);
            }
        }

        private void Nudge(int direction)
        {
            if (content == null)
            {
                return;
            }

            isSnapping = true;
            var position = content.anchoredPosition;
            position.x = Mathf.Clamp(position.x + direction * -cardWidth, -GetMaxScrollOffset(), 0f);
            content.DOAnchorPos(position, snapDuration).SetEase(Ease.OutCubic).OnComplete(() => isSnapping = false);
        }

        private float GetMaxScrollOffset()
        {
            if (scrollRect == null || scrollRect.viewport == null || content == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, content.rect.width - scrollRect.viewport.rect.width);
        }

        private void OnScrollValueChanged(Vector2 _)
        {
            UpdateFadeVisibility();
        }

        private void UpdateFadeVisibility()
        {
            if (content == null) return;

            var maxOffset = GetMaxScrollOffset();
            var currentX = -content.anchoredPosition.x;

            if (leftFade != null)
            {
                leftFade.gameObject.SetActive(currentX > 1f);
            }

            if (rightFade != null)
            {
                rightFade.gameObject.SetActive(currentX < maxOffset - 1f);
            }
        }
    }
}
