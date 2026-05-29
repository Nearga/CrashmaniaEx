using System.Collections.Generic;
using System.Linq;
using Crashmania.Models;
using PureMVC.Patterns.Proxy;

namespace Crashmania.PureMvc.Proxies
{
    public sealed class CatalogProxy : Proxy
    {
        public const string Name = "CatalogProxy";

        private LobbyDataResponse data = new();

        public CatalogProxy() : base(Name)
        {
        }

        public IReadOnlyList<CategoryModel> Categories => data.Categories;
        public IReadOnlyList<GameModel> TopGames => data.TopGames;
        public IReadOnlyList<BannerModel> Banners => data.Banners;

        public void SetData(LobbyDataResponse response)
        {
            data = response ?? new LobbyDataResponse();
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
