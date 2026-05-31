using Crashmania.Config;
using Crashmania.Core;
using Crashmania.Game;
using Crashmania.Models;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
using PureMVC.Interfaces;
using UnityEngine;

namespace Crashmania.PureMvc.Scenes
{
    public sealed class GameSceneController : MonoBehaviour, IPureMvcScene
    {
        [SerializeField] private MonoBehaviour gameControllerBehaviour;

        private IGameController gameController;
        private IFacade facade;

        private void Awake()
        {
            gameController = gameControllerBehaviour as IGameController;
            if (gameController == null)
            {
                gameController = GetComponentInChildren<IGameController>(true);
            }
        }

        public void Show(IFacade activeFacade)
        {
            facade = activeFacade;
            if (facade == null || gameController == null)
            {
                Debug.LogError("[GameSceneController] Game scene is missing a valid controller.");
                return;
            }

            Close(facade);

            var activeGameProxy = facade.RetrieveProxy(ActiveGameProxy.Name) as ActiveGameProxy;
            if (activeGameProxy == null)
            {
                Debug.LogError("[GameSceneController] ActiveGameProxy is not registered.");
                return;
            }

            if (!activeGameProxy.HasActiveGame)
            {
                activeGameProxy.SetActiveGame(CreateDevGame(), CreateDevSession());
            }

            gameController.OnRequestExit += OnRequestExit;
            gameController.OnBalanceChanged += OnBalanceChanged;

            var settings = facade.RetrieveProxy(SettingsProxy.Name) as SettingsProxy;
            gameController.Initialize(activeGameProxy.Session, settings);

            var balance = facade.RetrieveProxy(BalanceProxy.Name) as BalanceProxy;
            EnsureDevBalance(balance);
            if (balance != null)
            {
                gameController.OnBalanceUpdated(balance.BalanceCC, balance.BalanceSC);
            }

            Debug.Log("[GameSceneController] Game scene shown.");
        }

        public void Close(IFacade activeFacade)
        {
            if (gameController != null)
            {
                gameController.OnRequestExit -= OnRequestExit;
                gameController.OnBalanceChanged -= OnBalanceChanged;
                gameController.Shutdown();
            }
        }

        private void OnRequestExit()
        {
            facade?.SendNotification(LobbyNotifications.ExitGame);
        }

        private void OnBalanceChanged(double ccDelta, double scDelta)
        {
            var balance = facade?.RetrieveProxy(BalanceProxy.Name) as BalanceProxy;
            balance?.Credit(ccDelta, scDelta);
            if (balance != null)
            {
                gameController.OnBalanceUpdated(balance.BalanceCC, balance.BalanceSC);
            }
        }

        private static void EnsureDevBalance(BalanceProxy balance)
        {
            if (balance == null || balance.BalanceCC > 0.0 || balance.BalanceSC > 0.0)
            {
                return;
            }

            var config = ServiceLocator.Resolve<AppConfig>();
            balance.Initialize(
                config != null ? config.startingBalanceCC : 250000.0,
                config != null ? config.startingBalanceSC : 5.0);
        }

        private static GameModel CreateDevGame()
        {
            return new GameModel
            {
                Id = "dev-crash",
                Name = "Crash",
                SceneAddress = "Game"
            };
        }

        private static GameSession CreateDevSession()
        {
            return new GameSession
            {
                SessionId = "dev-game-session",
                GameId = "dev-crash",
                AccessToken = "dev-access-token"
            };
        }
    }
}
