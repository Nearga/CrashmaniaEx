using Crashmania.PureMvc.Commands.Auth;
using Crashmania.PureMvc.Commands.Lobby;
using Crashmania.PureMvc.Commands.Navigation;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
using PureMVC.Patterns.Facade;

namespace Crashmania.PureMvc
{
    public sealed class LobbyFacade : Facade
    {
        private bool started;

        static LobbyFacade()
        {
            instance = new LobbyFacade();
        }

        public static LobbyFacade GetInstance()
        {
            return instance as LobbyFacade;
        }

        public void Startup()
        {
            if (started)
            {
                return;
            }

            RegisterProxy(new AuthProxy());
            RegisterProxy(new CatalogProxy());
            RegisterCommand(LobbyNotifications.LoginRequest, () => new LoginCommand());
            RegisterCommand(LobbyNotifications.LoadLobbyData, () => new LoadLobbyDataCommand());
            RegisterCommand(LobbyNotifications.NavigateTo, () => new NavigateCommand());
            RegisterCommand(LobbyNotifications.SceneLoaded, () => new SceneLoadedCommand());
            started = true;
        }
    }
}
