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
        [SerializeField] private ScrollRect categoryScrollRect;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private RecentMultipliersView recentMultipliersView;
        [SerializeField] private StorePanelView storePanelView;

        [Header("Shell Tab Panels")]
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private GameObject storePanel;
        [SerializeField] private GameObject giftsPanel;
        [SerializeField] private GameObject accountPanel;

        private readonly List<CategoryChipView> chips = new();
        private LobbyDataResponse currentData;
        private string activeCategoryId = "all";
        private Coroutine searchDebounceRoutine;

        public event Action<string> CategorySelected;
        public event Action<string> GameSelected;
        public event Action<string> ViewAllSelected;
        public event Action<string> SearchChanged;
        public event Action<string> PurchaseRequested;

        private void Awake()
        {
            if (promoBanner != null)
            {
                promoBanner.CtaClicked += id => GameSelected?.Invoke(id);
            }

            if (storePanelView != null)
            {
                storePanelView.PurchaseRequested += id => PurchaseRequested?.Invoke(id);
            }

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
            if (data != null && IsDataIdentical(data))
            {
                return;
            }

            currentData = data ?? new LobbyDataResponse();
            activeCategoryId = "all";
            RenderPromo();
            RenderRecentMultipliers();
            RenderCategories();
            RenderCarousels();
        }

        private bool IsDataIdentical(LobbyDataResponse data)
        {
            if (currentData == null) return false;
            
            // Compare top games count
            if (currentData.TopGames.Count != data.TopGames.Count) return false;
            
            // Compare categories count
            if (currentData.Categories.Count != data.Categories.Count) return false;
            
            // Compare banners count
            if (currentData.Banners.Count != data.Banners.Count) return false;

            // Deep compare Banners
            for (int i = 0; i < currentData.Banners.Count; i++)
            {
                var b1 = currentData.Banners[i];
                var b2 = data.Banners[i];
                if (b1.Id != b2.Id || b1.Title != b2.Title || b1.ImageResourcePath != b2.ImageResourcePath)
                {
                    return false;
                }
            }

            // Deep compare Categories
            for (int i = 0; i < currentData.Categories.Count; i++)
            {
                var c1 = currentData.Categories[i];
                var c2 = data.Categories[i];
                if (c1.Id != c2.Id || c1.Name != c2.Name || c1.Games.Count != c2.Games.Count)
                {
                    return false;
                }

                for (int j = 0; j < c1.Games.Count; j++)
                {
                    var g1 = c1.Games[j];
                    var g2 = c2.Games[j];
                    if (g1.Id != g2.Id || g1.Name != g2.Name || g1.OnlineCount != g2.OnlineCount || g1.ThumbnailResourcePath != g2.ThumbnailResourcePath)
                    {
                        return false;
                    }
                }
            }

            return true;
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

            // Reset chip scroll to left edge so first chip is visible
            if (categoryScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                categoryScrollRect.horizontalNormalizedPosition = 0f;
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

        public void RenderStore(List<StorePackage> packages)
        {
            if (storePanelView != null)
            {
                storePanelView.Render(packages);
            }
        }

        public void ShowTab(string tabName)
        {
            if (lobbyPanel != null) lobbyPanel.SetActive(tabName == "Lobby");
            if (storePanel != null) storePanel.SetActive(tabName == "Store");
            if (giftsPanel != null) giftsPanel.SetActive(tabName == "Gifts");
            if (accountPanel != null) accountPanel.SetActive(tabName == "Account");
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
