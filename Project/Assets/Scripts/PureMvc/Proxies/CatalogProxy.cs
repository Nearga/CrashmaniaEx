using System.Collections.Generic;
using System.Linq;
using Crashmania.Models;
using PureMVC.Patterns.Proxy;

namespace Crashmania.PureMvc.Proxies
{
    public sealed class CatalogProxy : Proxy
    {
        public const string Name = "CatalogProxy";

        private const string CacheKey = "cached_lobby_data";
        private LobbyDataResponse data = new();

        public CatalogProxy() : base(Name)
        {
            LoadFromCache();
        }

        public IReadOnlyList<CategoryModel> Categories => data.Categories;
        public IReadOnlyList<GameModel> TopGames => data.TopGames;
        public IReadOnlyList<BannerModel> Banners => data.Banners;

        public void SetData(LobbyDataResponse response)
        {
            data = response ?? new LobbyDataResponse();
            SaveToCache();
        }

        private void LoadFromCache()
        {
            try
            {
                var cachedJson = UnityEngine.PlayerPrefs.GetString(CacheKey, string.Empty);
                if (!string.IsNullOrEmpty(cachedJson))
                {
                    var cachedData = UnityEngine.JsonUtility.FromJson<LobbyDataResponse>(cachedJson);
                    if (cachedData != null && cachedData.Categories != null && cachedData.Categories.Count > 0)
                    {
                        data = cachedData;
                    }
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[CatalogProxy] Failed to load lobby data from cache: {ex.Message}");
            }
        }

        private void SaveToCache()
        {
            try
            {
                if (data != null && data.Categories != null && data.Categories.Count > 0)
                {
                    var json = UnityEngine.JsonUtility.ToJson(data);
                    UnityEngine.PlayerPrefs.SetString(CacheKey, json);
                    UnityEngine.PlayerPrefs.Save();
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[CatalogProxy] Failed to save lobby data to cache: {ex.Message}");
            }
        }

        public IReadOnlyList<GameModel> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Categories.SelectMany(category => category.Games).Distinct().ToList();
            }

            var normalized = query.Trim().ToLowerInvariant();
            return Categories
                .SelectMany(category => category.Games)
                .Where(game => game.Name != null && game.Name.ToLowerInvariant().Contains(normalized))
                .Distinct()
                .ToList();
        }

        public CategoryModel GetCategory(string id)
        {
            return Categories.FirstOrDefault(category => category.Id == id);
        }

        public GameModel GetGame(string id)
        {
            return Categories.SelectMany(category => category.Games).FirstOrDefault(game => game.Id == id);
        }
    }
}
