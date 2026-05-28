using Crashmania.Config;
using Crashmania.Core;
using Crashmania.PureMvc.Mediators;
using Crashmania.UI.Login;
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
                RegisterLoginMediator();
                return;
            }

            RemoveMediatorIfPresent(LoginMediator.Name);
            Debug.Log($"[SceneLoadedCommand] Scene loaded: {sceneName}");
        }

        private void RegisterLoginMediator()
        {
            RemoveMediatorIfPresent(LoginMediator.Name);

            var view = Object.FindAnyObjectByType<LoginView>(FindObjectsInactive.Include) ?? 
                Object.Instantiate(Resources.Load<GameObject>("UI/Prefabs/LoginScreen")).GetComponent<LoginView>();
            Facade.RegisterMediator(new LoginMediator(view));
            Debug.Log("[SceneLoadedCommand] Login mediator registered.");
        }

        private void RemoveMediatorIfPresent(string mediatorName)
        {
            if (Facade.HasMediator(mediatorName))
            {
                Facade.RemoveMediator(mediatorName);
            }
        }

        private static T TryResolve<T>() where T : class
        {
            try
            {
                return ServiceLocator.Resolve<T>();
            }
            catch
            {
                return null;
            }
        }
    }
}
