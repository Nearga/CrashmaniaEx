using System;
using System.Collections.Generic;
using Crashmania.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Lobby
{
    public sealed class LobbyView : MonoBehaviour
    {
        [SerializeField] private PromoBannerView promoBanner;
        [SerializeField] private CategoryChipView categoryChipPrefab;
        [SerializeField] private GamesCarouselView carouselPrefab;
        [SerializeField] private GameCardView gameCardPrefab;
        [SerializeField] private GameCardView topGameCardPrefab;
        [SerializeField] private RectTransform categoryContent;
        [SerializeField] private RectTransform carouselContent;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private TMP_Text recentMultipliersText;

        private readonly List<CategoryChipView> chips = new();
        private LobbyDataResponse currentData;
        private string activeCategoryId = "all";

        public event Action<string> CategorySelected;
        public event Action<string> GameSelected;
        public event Action<string> ViewAllSelected;
        public event Action<string> SearchChanged;

        private void Awake()
        {
            if (promoBanner == null) promoBanner = transform.Find("ScrollRect/Viewport/Content/PromoSection/MainPromo")?.GetComponent<PromoBannerView>();
            if (categoryContent == null) categoryContent = transform.Find("ScrollRect/Viewport/Content/CategoryRail/ScrollRect/Viewport/Content")?.GetComponent<RectTransform>();
            if (carouselContent == null) carouselContent = transform.Find("ScrollRect/Viewport/Content/CarouselSections")?.GetComponent<RectTransform>();
            if (searchInput == null) searchInput = transform.Find("ScrollRect/Viewport/Content/CategoryRail/SearchInput")?.GetComponent<TMP_InputField>();
            if (recentMultipliersText == null) recentMultipliersText = transform.Find("ScrollRect/Viewport/Content/RecentMultipliers/Text")?.GetComponent<TMP_Text>();

            if (searchInput != null)
            {
                searchInput.onValueChanged.AddListener(OnSearchChanged);
            }
        }

        private void OnDestroy()
        {
            if (searchInput != null)
            {
                searchInput.onValueChanged.RemoveListener(OnSearchChanged);
            }
        }

        public void Render(LobbyDataResponse data)
        {
            currentData = data ?? new LobbyDataResponse();
            activeCategoryId = "all";
            RenderPromo();
            RenderRecentMultipliers();
            RenderCategories();
            RenderCarousels();
        }

        public void RenderSearchResults(IReadOnlyList<GameModel> games)
        {
            if (carouselContent == null || carouselPrefab == null || gameCardPrefab == null)
            {
                return;
            }

            Clear(carouselContent);
            var carousel = Instantiate(carouselPrefab, carouselContent);
            carousel.BindSearchResults("SEARCH RESULTS", games, gameCardPrefab);
            carousel.GameSelected += id => GameSelected?.Invoke(id);
            carousel.ViewAllRequested += id => ViewAllSelected?.Invoke(id);
        }

        private void RenderPromo()
        {
            if (promoBanner != null && currentData.Banners.Count > 0)
            {
                promoBanner.Bind(currentData.Banners[0]);
            }
        }

        private void RenderRecentMultipliers()
        {
            if (recentMultipliersText != null)
            {
                recentMultipliersText.text = "RECENT MULTIPLIERS:  <color=#11D950>1.25x</color>     <color=#11D950>1.14x</color>     <color=#11D950>1.29x</color>     <color=#11D950>1.11x</color>     <color=#E93628>1.3x</color>";
            }
        }

        private void RenderCategories()
        {
            if (categoryContent == null || categoryChipPrefab == null)
            {
                return;
            }

            Clear(categoryContent);
            chips.Clear();
            foreach (var category in currentData.Categories)
            {
                var chip = Instantiate(categoryChipPrefab, categoryContent);
                chip.Bind(category.Id, category.Name, category.Id == activeCategoryId);
                chip.Selected += OnCategorySelected;
                chips.Add(chip);
            }
        }

        private void RenderCarousels()
        {
            if (carouselContent == null || carouselPrefab == null || gameCardPrefab == null)
            {
                return;
            }

            Clear(carouselContent);
            foreach (var category in currentData.Categories)
            {
                if (category.Id == "all" || category.Id == "trending" || category.Games.Count == 0)
                {
                    continue;
                }

                var carousel = Instantiate(carouselPrefab, carouselContent);
                carousel.Bind(category, gameCardPrefab, topGameCardPrefab);
                carousel.GameSelected += id => GameSelected?.Invoke(id);
                carousel.ViewAllRequested += id => ViewAllSelected?.Invoke(id);
            }
        }

        private void OnCategorySelected(string id)
        {
            activeCategoryId = id;
            foreach (var chip in chips)
            {
                chip.SetActive(chip.CategoryId == id);
            }

            CategorySelected?.Invoke(id);
        }

        private void OnSearchChanged(string query)
        {
            SearchChanged?.Invoke(query);
        }

        private static void Clear(Transform target)
        {
            for (var index = target.childCount - 1; index >= 0; index--)
            {
                Destroy(target.GetChild(index).gameObject);
            }
        }
    }
}
