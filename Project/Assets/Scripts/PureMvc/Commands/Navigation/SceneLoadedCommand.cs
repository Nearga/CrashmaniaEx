using Crashmania.Core;
using Crashmania.PureMvc.Scenes;
using Crashmania.UI.Shell;
using Crashmania.Config;
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
            
            var tokens = DependencyContainer.Instance.Resolve<DesignTokens>();
            var config = ServiceLocator.Resolve<AppConfig>();
            ShellBootstrapper.EnsureShell(tokens, config, Facade as LobbyFacade);

            if (!PureMvcSceneRegistry.ShowActiveScene(Facade))
            {
                Debug.Log($"[SceneLoadedCommand] Scene loaded without PureMVC scene controller: {sceneName}");
            }
        }
    }
}
