using Crashmania.PureMvc.Notifications;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

namespace Crashmania.PureMvc.Commands.Lobby
{
    public sealed class LaunchGameCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            var gameId = notification.Body as string;
            
            // For now, just navigate to the Game scene.
            // In the future, this would set the active game in a proxy.
            SendNotification(LobbyNotifications.NavigateToScene, "Game");
        }
    }
}
