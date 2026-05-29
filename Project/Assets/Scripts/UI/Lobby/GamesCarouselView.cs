using System;
using System.Collections.Generic;
using Crashmania.Models;
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
        [SerializeField] private RectTransform content;

        private string categoryId;

        public event Action<string> ViewAllRequested;
        public event Action<string> GameSelected;

        private void Awake()
        {
            if (titleText == null) titleText = transform.Find("Header/Title")?.GetComponent<TMP_Text>();
            if (viewAllButton == null) viewAllButton = transform.Find("Header/ViewAllButton")?.GetComponent<Button>();
            if (previousButton == null) previousButton = transform.Find("Header/PreviousButton")?.GetComponent<Button>();
            if (nextButton == null) nextButton = transform.Find("Header/NextButton")?.GetComponent<Button>();
            if (content == null) content = transform.Find("ScrollRect/Viewport/Content")?.GetComponent<RectTransform>();

            if (viewAllButton != null) viewAllButton.onClick.AddListener(() => ViewAllRequested?.Invoke(categoryId));
            if (previousButton != null) previousButton.onClick.AddListener(() => Nudge(-1));
            if (nextButton != null) nextButton.onClick.AddListener(() => Nudge(1));
        }

        private void OnDestroy()
        {
            if (viewAllButton != null) viewAllButton.onClick.RemoveAllListeners();
            if (previousButton != null) previousButton.onClick.RemoveAllListeners();
            if (nextButton != null) nextButton.onClick.RemoveAllListeners();
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

            var position = content.anchoredPosition;
            position.x += direction * -320f;
            content.anchoredPosition = position;
        }
    }
}
