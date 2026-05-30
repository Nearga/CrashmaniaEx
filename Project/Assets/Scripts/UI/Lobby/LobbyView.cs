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
        [SerializeField] private RecentMultipliersView recentMultipliersView;

        private readonly List<CategoryChipView> chips = new();
        private LobbyDataResponse currentData;
        private string activeCategoryId = "all";
        private Coroutine searchDebounceRoutine;

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
            if (recentMultipliersView == null) recentMultipliersView = transform.Find("ScrollRect/Viewport/Content/RecentMultipliers")?.GetComponent<RecentMultipliersView>();

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

            if (searchDebounceRoutine != null)
            {
                StopCoroutine(searchDebounceRoutine);
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
            RebuildScrollableLayout();
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
            // RecentMultipliersView.Start() populates itself with default data;
            // when real multiplier data is available, call recentMultipliersView.Bind(values).
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
                chip.Bind(category.Id, category.Name.ToUpper(), category.Id == activeCategoryId);
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

            RebuildScrollableLayout();
        }

        private void OnCategorySelected(string id)
        {
            activeCategoryId = id;
            if (searchInput != null)
            {
                searchInput.SetTextWithoutNotify(string.Empty);
            }

            foreach (var chip in chips)
            {
                chip.SetActive(chip.CategoryId == id);
            }

            CategorySelected?.Invoke(id);
        }

        private void OnSearchChanged(string query)
        {
            if (searchDebounceRoutine != null)
            {
                StopCoroutine(searchDebounceRoutine);
            }

            searchDebounceRoutine = StartCoroutine(DispatchSearchChanged(query));
        }

        private System.Collections.IEnumerator DispatchSearchChanged(string query)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            searchDebounceRoutine = null;
            SearchChanged?.Invoke(query);
        }

        private void RebuildScrollableLayout()
        {
            if (carouselContent == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(carouselContent);

            var layoutElement = carouselContent.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.preferredHeight = Mathf.Max(0f, LayoutUtility.GetPreferredHeight(carouselContent));
            }

            var contentRoot = carouselContent.parent as RectTransform;
            if (contentRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            }

            Canvas.ForceUpdateCanvases();
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
