using Crashmania.PureMvc.Commands.Auth;
using Crashmania.PureMvc.Commands.Game;
using Crashmania.PureMvc.Commands.Lobby;
using Crashmania.PureMvc.Commands.Navigation;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
using PureMVC.Patterns.Facade;
using UnityEngine;

namespace Crashmania.PureMvc
{
    public sealed class LobbyFacade : Facade
    {
        private bool started;

        static LobbyFacade()
        {
            instance = new LobbyFacade();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            if (instance is LobbyFacade facade)
            {
                facade.started = false;
            }
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
            RegisterProxy(new BalanceProxy());
            RegisterProxy(new SettingsProxy());
            RegisterProxy(new ActiveGameProxy());
            RegisterCommand(LobbyNotifications.LoginRequest, () => new LoginCommand());
            RegisterCommand(LobbyNotifications.LoadLobbyData, () => new LoadLobbyDataCommand());
            RegisterCommand(LobbyNotifications.NavigateToScene, () => new NavigateSceneCommand());
            RegisterCommand(LobbyNotifications.NavigateToTab, () => new NavigateLobbyTabCommand());
            RegisterCommand(LobbyNotifications.SceneLoaded, () => new SceneLoadedCommand());
            RegisterCommand(LobbyNotifications.LaunchGame, () => new LaunchGameCommand());
            RegisterCommand(LobbyNotifications.ExitGame, () => new ExitGameCommand());
            RegisterCommand(LobbyNotifications.PurchaseItem, () => new PurchaseStoreItemCommand());
            started = true;
        }
    }
}
