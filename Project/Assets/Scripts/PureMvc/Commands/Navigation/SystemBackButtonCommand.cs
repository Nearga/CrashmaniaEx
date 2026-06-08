using Crashmania.PureMvc.Mediators;
using Crashmania.PureMvc.Notifications;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

namespace Crashmania.PureMvc.Commands.Navigation
{
    public sealed class SystemBackButtonCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            // 1. Check Modals
            var modalMediator = Facade.RetrieveMediator(ModalMediator.Name) as ModalMediator;
            if (modalMediator != null && modalMediator.IsModalOpen)
            {
                SendNotification(LobbyNotifications.HideModal);
                return;
            }

            // 2. Check Scene/Context
            // For now, if no modal, go to Lobby tab (default behavior)
            SendNotification(LobbyNotifications.NavigateToTab, "Lobby");
        }
    }
}
