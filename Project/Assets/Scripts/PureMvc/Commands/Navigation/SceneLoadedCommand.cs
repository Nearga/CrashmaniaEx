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
            Debug.Log($"[SceneLoadedCommand] Scene loaded: {sceneName}");
        }
    }
}
