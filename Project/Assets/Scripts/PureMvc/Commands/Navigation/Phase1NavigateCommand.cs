using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

namespace Crashmania.PureMvc.Commands.Navigation
{
    public sealed class Phase1NavigateCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            var sceneName = notification.Body as string;
            Debug.Log($"[LobbyFacade] Phase 1 navigation smoke: {sceneName}");
        }
    }
}
