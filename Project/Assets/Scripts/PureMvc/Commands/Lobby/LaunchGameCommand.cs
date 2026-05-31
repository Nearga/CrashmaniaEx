using Crashmania.Core;
using Crashmania.Models;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
using Crashmania.Services;
using Cysharp.Threading.Tasks;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

namespace Crashmania.PureMvc.Commands.Lobby
{
    public sealed class LaunchGameCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            Launch(notification.Body).Forget();
        }

        private async UniTaskVoid Launch(object body)
        {
            try
            {
                var game = ResolveGame(body);
                if (game == null)
                {
                    SendNotification(LobbyNotifications.ShowToast, "Game not found.");
                    return;
                }

                var backend = ServiceLocator.Resolve<IBackendService>();
                var loader = ServiceLocator.Resolve<IGameLoader>();
                var auth = Facade.RetrieveProxy(AuthProxy.Name) as AuthProxy;
                var activeGame = Facade.RetrieveProxy(ActiveGameProxy.Name) as ActiveGameProxy;

                if (backend == null || loader == null || activeGame == null)
                {
                    Debug.LogError("[LaunchGameCommand] Missing backend, game loader, or active game proxy.");
                    SendNotification(LobbyNotifications.ShowToast, "Game loader is not ready.");
                    return;
                }

                var session = await backend.StartGameSession(game.Id, auth != null ? auth.AccessToken : string.Empty);
                activeGame.SetActiveGame(game, session);

                await loader.LoadGame(game);
                SendNotification(LobbyNotifications.GameLoaded, game);
                SendNotification(LobbyNotifications.SceneLoaded, "Game");
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[LaunchGameCommand] Failed to launch game: {exception}");
                SendNotification(LobbyNotifications.ShowToast, "Failed to load game.");
            }
        }

        private GameModel ResolveGame(object body)
        {
            if (body is GameModel model)
            {
                return model;
            }

            var gameId = body as string;
            var catalog = Facade.RetrieveProxy(CatalogProxy.Name) as CatalogProxy;
            if (!string.IsNullOrWhiteSpace(gameId))
            {
                return catalog?.GetGame(gameId);
            }

            if (catalog != null && catalog.TopGames.Count > 0)
            {
                return catalog.TopGames[0];
            }

            return null;
        }
    }
}
