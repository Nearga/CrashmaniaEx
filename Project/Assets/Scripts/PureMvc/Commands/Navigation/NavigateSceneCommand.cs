using Crashmania.Core;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Scenes;
using Crashmania.Services;
using Cysharp.Threading.Tasks;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

namespace Crashmania.PureMvc.Commands.Navigation
{
    public sealed class NavigateSceneCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            var sceneName = notification.Body as string;
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("[NavigateSceneCommand] Navigation ignored: target scene name is empty.");
                return;
            }

            Load(sceneName).Forget();
        }

        private static async UniTaskVoid Load(string sceneName)
        {
            try
            {
                var navigationService = ServiceLocator.Resolve<NavigationService>();
                var facade = LobbyFacade.GetInstance();

                PureMvcSceneRegistry.CloseActiveScene(facade);
                await navigationService.LoadScene(sceneName, showTransition: true);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[NavigateSceneCommand] Failed to navigate to scene {sceneName}: {exception}");
            }
        }
    }
}
