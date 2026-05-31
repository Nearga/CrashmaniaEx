using Crashmania.Core;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
using Crashmania.PureMvc.Scenes;
using Crashmania.Services;
using Cysharp.Threading.Tasks;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

namespace Crashmania.PureMvc.Commands.Game
{
    public sealed class ExitGameCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            Exit().Forget();
        }

        private async UniTaskVoid Exit()
        {
            try
            {
                PureMvcSceneRegistry.CloseActiveScene(Facade);

                var loader = ServiceLocator.Resolve<IGameLoader>();
                if (loader != null)
                {
                    await loader.UnloadGame();
                }

                var activeGame = Facade.RetrieveProxy(ActiveGameProxy.Name) as ActiveGameProxy;
                activeGame?.Clear();

                SendNotification(LobbyNotifications.NavigateToTab, "Lobby");
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[ExitGameCommand] Failed to exit game: {exception}");
                SendNotification(LobbyNotifications.ShowToast, "Could not exit game.");
            }
        }
    }
}
