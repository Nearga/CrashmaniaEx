using System.Collections.Generic;
using Crashmania.Models;

namespace Crashmania.Services
{
    public static class MockCatalog
    {
        public static LobbyDataResponse Create()
        {
            var luckyWeek = new List<GameModel>
            {
                Game("lucky-twins", "Lucky Twins", "UI/Games/Top10/1", 82),
                Game("lucky-apple", "Lucky Apple", "UI/Games/Top10/2", 91),
                Game("lucky-lion", "Lucky Lion", "UI/Games/Top10/3", 64),
                Game("bountiful-birds", "Bountiful Birds", "UI/Games/Homepage/bountiful-birds", 58),
                Game("rise-up", "Rise Up", "UI/Games/Homepage/rise_up", 73)
            };

            var crashGames = new List<GameModel>
            {
                Game("tiltx", "Tilt X", "UI/Games/Homepage/tiltx", 44),
                Game("astro-go", "Astro Go", "UI/Games/Homepage/astro_go", 69),
                Game("skyride", "Skyride", "UI/Games/Homepage/skyride", 57),
                Game("swoosh-up", "Swoosh Up", "UI/Games/Homepage/swoosh_up", 61),
                Game("crush-depth", "Crush Depth", "UI/Games/Homepage/crush_depth", 38)
            };

            var hotGames = new List<GameModel>
            {
                Game("fight-x", "Fight X", "UI/Games/Homepage/fightX", 112),
                Game("moon-juggling", "Moon Juggling", "UI/Games/Homepage/moon_juggling", 87),
                Game("slackliner", "Slackliner", "UI/Games/Homepage/slackliner", 52),
                Game("astro-go-hot", "Astro Go", "UI/Games/Homepage/astro_go", 96),
                Game("tiltx-hot", "Tilt X", "UI/Games/Homepage/tiltx", 75)
            };

            return new LobbyDataResponse
            {
                Banners = new List<BannerModel>
                {
                    Banner("mission", "Daily Mission", "UI/Promotions/Lobby/mission"),
                    Banner("lobby-bg", "Lucky Twins", "UI/Promotions/Lobby/lobby-bg"),
                    Banner("front", "Welcome Offer", "UI/Promotions/Lobby/front-image"),
                    Banner("gift", "Gift", "UI/Promotions/Lobby/gift"),
                    Banner("gift-sweep", "Gift Sweep", "UI/Promotions/Lobby/gift-sweep")
                },
                TopGames = luckyWeek,
                Categories = new List<CategoryModel>
                {
                    Category("all", "ALL", Combine(luckyWeek, crashGames, hotGames)),
                    Category("lucky-week", "LUCKY WEEK", luckyWeek),
                    Category("crash-games", "CRASH GAMES", crashGames),
                    Category("trending", "TRENDING", hotGames),
                    Category("hot-games", "HOT GAMES", hotGames)
                }
            };
        }

        private static GameModel Game(string id, string name, string resourcePath, int onlineCount)
        {
            return new GameModel
            {
                Id = id,
                Name = name,
                SceneAddress = "Game",
                ThumbnailResourcePath = resourcePath,
                OnlineCount = onlineCount
            };
        }

        private static BannerModel Banner(string id, string title, string resourcePath)
        {
            return new BannerModel
            {
                Id = id,
                Title = title,
                ImageResourcePath = resourcePath
            };
        }

        private static CategoryModel Category(string id, string name, List<GameModel> games)
        {
            return new CategoryModel { Id = id, Name = name, Games = games };
        }

        private static List<GameModel> Combine(params List<GameModel>[] gameLists)
        {
            var combined = new List<GameModel>();
            foreach (var list in gameLists)
            {
                combined.AddRange(list);
            }

            return combined;
        }
    }
}
