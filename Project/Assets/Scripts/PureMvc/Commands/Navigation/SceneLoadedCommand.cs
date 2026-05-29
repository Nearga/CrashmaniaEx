using Crashmania.PureMvc.Mediators;
using Crashmania.PureMvc.Notifications;
using Crashmania.UI.Login;
using Crashmania.UI.Lobby;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

namespace Crashmania.PureMvc.Commands.Navigation
{
    public sealed class SceneLoadedCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            var sceneName = notification.Body as string;
            if (sceneName == "Login")
            {
                RemoveMediatorIfPresent(LobbyMediator.Name);
                RegisterLoginMediator();
                return;
            }

            if (sceneName == "Lobby")
            {
                RemoveMediatorIfPresent(LoginMediator.Name);
                RegisterLobbyMediator();
                SendNotification(LobbyNotifications.LoadLobbyData);
                return;
            }

            RemoveMediatorIfPresent(LoginMediator.Name);
            RemoveMediatorIfPresent(LobbyMediator.Name);
            Debug.Log($"[SceneLoadedCommand] Scene loaded: {sceneName}");
        }

        private void RegisterLoginMediator()
        {
            RemoveMediatorIfPresent(LoginMediator.Name);

            var view = Object.FindAnyObjectByType<LoginView>(FindObjectsInactive.Include);
            if (view == null)
            {
                Debug.LogError("[SceneLoadedCommand] Login scene is missing an in-scene LoginView.");
                return;
            }

            Facade.RegisterMediator(new LoginMediator(view));
            Debug.Log("[SceneLoadedCommand] Login mediator registered.");
        }

        private void RegisterLobbyMediator()
        {
            RemoveMediatorIfPresent(LobbyMediator.Name);

            var view = Object.FindAnyObjectByType<LobbyView>(FindObjectsInactive.Include);
            if (view == null)
            {
                Debug.LogError("[SceneLoadedCommand] Lobby scene is missing an in-scene LobbyView.");
                return;
            }

            Facade.RegisterMediator(new LobbyMediator(view));
            Debug.Log("[SceneLoadedCommand] Lobby mediator registered.");
        }

        private void RemoveMediatorIfPresent(string mediatorName)
        {
            if (Facade.HasMediator(mediatorName))
            {
                Facade.RemoveMediator(mediatorName);
            }
        }
    }
}
