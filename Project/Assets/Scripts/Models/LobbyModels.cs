using System.Collections.Generic;

namespace Crashmania.Models
{
    public sealed class LobbyDataResponse
    {
        public List<CategoryModel> Categories = new();
        public List<GameModel> TopGames = new();
        public List<BannerModel> Banners = new();
    }

    public sealed class CategoryModel
    {
        public string Id;
        public string Name;
        public List<GameModel> Games = new();
    }

    public sealed class GameModel
    {
        public string Id;
        public string Name;
        public string SceneAddress;
        public string ThumbnailUrl;
        public string ThumbnailResourcePath;
        public int OnlineCount;
    }

    public sealed class BannerModel
    {
        public string Id;
        public string Title;
        public string ImageUrl;
        public string ImageResourcePath;
    }
}
