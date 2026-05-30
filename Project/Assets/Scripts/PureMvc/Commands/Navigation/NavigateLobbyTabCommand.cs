using Crashmania.Core;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Scenes;
using Crashmania.Services;
using Cysharp.Threading.Tasks;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Crashmania.PureMvc.Commands.Navigation
{
    public sealed class NavigateLobbyTabCommand : SimpleCommand
    {
        private const string LobbySceneName = "Lobby";

        public override void Execute(INotification notification)
        {
            var tabName = notification.Body as string;
            if (string.IsNullOrWhiteSpace(tabName))
            {
                Debug.LogWarning("[NavigateLobbyTabCommand] Navigation ignored: target tab name is empty.");
                return;
            }

            Load(tabName).Forget();
        }

        private static async UniTaskVoid Load(string tabName)
        {
            try
            {
                var navigationService = ServiceLocator.Resolve<NavigationService>();
                var facade = LobbyFacade.GetInstance();

                navigationService.TargetTab = tabName;

                // If already inside the Lobby shell, switch sub-panels instantly
                if (SceneManager.GetActiveScene().name == LobbySceneName)
                {
                    facade.SendNotification(LobbyNotifications.ShowTab, tabName);
                    facade.SendNotification(LobbyNotifications.SceneLoaded, tabName);
                    return;
                }

                PureMvcSceneRegistry.CloseActiveScene(facade);
                await navigationService.LoadScene(LobbySceneName, showTransition: true);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[NavigateLobbyTabCommand] Failed to navigate to tab {tabName}: {exception}");
            }
        }
    }
}
