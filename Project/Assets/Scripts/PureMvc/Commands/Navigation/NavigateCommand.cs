using Crashmania.Core;
using Crashmania.PureMvc.Scenes;
using Crashmania.Services;
using Cysharp.Threading.Tasks;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

namespace Crashmania.PureMvc.Commands.Navigation
{
    public sealed class NavigateCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            var sceneName = notification.Body as string;
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("[NavigateCommand] NavigateTo ignored because scene name is empty.");
                return;
            }

            Load(sceneName).Forget();
        }

        private static async UniTaskVoid Load(string sceneName)
        {
            try
            {
                PureMvcSceneRegistry.CloseActiveScene(LobbyFacade.GetInstance());

                var navigationService = ServiceLocator.Resolve<NavigationService>();
                await navigationService.LoadScene(sceneName);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[NavigateCommand] Failed to navigate to {sceneName}: {exception}");
            }
        }
    }
}
