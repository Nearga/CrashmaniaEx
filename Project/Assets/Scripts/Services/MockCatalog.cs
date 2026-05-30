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
                Game("lucky-twins", "Lucky Twins", "UI/NativeSprites/MGSlots-Lucky_Twins_Wilds_Jackpots", 82),
                Game("bountiful-birds", "Bountiful Birds", "UI/NativeSprites/MGSlots-Bountiful_Birds", 91),
                Game("african-wilds", "African Wilds", "UI/NativeSprites/MGSlots-African_Wilds", 64),
                Game("almighty-zeus", "Almighty Zeus", "UI/NativeSprites/MGSlots-Almighty_Zeus_Wilds", 58),
                Game("carnaval-fiesta", "Carnaval Fiesta", "UI/NativeSprites/MGSlots-Carnaval_Fiesta", 73)
            };

            var crashGames = new List<GameModel>
            {
                Game("tiltx", "Tilt X", "UI/Games/Homepage/tiltx", 44),
                Game("astro-go", "Astro Go", "UI/NativeSprites/Crash-astro_go_thumbnail", 69),
                Game("skyride", "Skyride", "UI/NativeSprites/Crash-skyride_thumbnail", 57),
                Game("swoosh-up", "Swoosh Up", "UI/Games/Homepage/swoosh_up", 61),
                Game("crush-depth", "Crush Depth", "UI/NativeSprites/Crash-crash_depth_thumbnail", 38)
            };

            var hotGames = new List<GameModel>
            {
                Game("fight-x", "Fight X", "UI/NativeSprites/Crash-fightX_thumbnail", 112),
                Game("moon-juggling", "Moon Juggling", "UI/Games/Homepage/moon_juggling", 87),
                Game("slackliner", "Slackliner", "UI/Games/Homepage/slackliner", 52),
                Game("astro-go-hot", "Astro Go", "UI/NativeSprites/Crash-astro_go_thumbnail", 96),
                Game("tiltx-hot", "Tilt X", "UI/Games/Homepage/tiltx", 75)
            };

            return new LobbyDataResponse
            {
                Banners = new List<BannerModel>
                {
                    Banner("front", "Lucky Twins", "UI/NativeSprites/MGSlots-Lucky_Twins_Wilds_Jackpots"),
                    Banner("mission", "Daily Mission", "UI/Promotions/Lobby/mission"),
                    Banner("lobby-bg", "Lucky Twins", "UI/Promotions/Lobby/lobby-bg"),
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
