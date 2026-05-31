using Crashmania.Models;
using PureMVC.Patterns.Proxy;

namespace Crashmania.PureMvc.Proxies
{
    public sealed class ActiveGameProxy : Proxy
    {
        public const string Name = "ActiveGameProxy";

        public ActiveGameProxy() : base(Name)
        {
        }

        public GameModel ActiveGame { get; private set; }
        public GameSession Session { get; private set; }
        public bool HasActiveGame => ActiveGame != null && Session != null;

        public void SetActiveGame(GameModel game, GameSession session)
        {
            ActiveGame = game;
            Session = session;
        }

        public void Clear()
        {
            ActiveGame = null;
            Session = null;
        }
    }
}
