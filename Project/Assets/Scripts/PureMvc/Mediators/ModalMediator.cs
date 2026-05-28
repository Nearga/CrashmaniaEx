using Crashmania.PureMvc.Notifications;
using Crashmania.UI.Shell;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;

namespace Crashmania.PureMvc.Mediators
{
    public sealed class ModalMediator : Mediator
    {
        public const string Name = "ModalMediator";

        private ModalView View => ViewComponent as ModalView;

        public ModalMediator(ModalView view) : base(Name, view)
        {
        }

        public override string[] ListNotificationInterests()
        {
            return new[] { LobbyNotifications.ShowModal, LobbyNotifications.HideModal };
        }

        public override void HandleNotification(INotification notification)
        {
            if (notification.Name == LobbyNotifications.ShowModal)
            {
                View.Show();
                return;
            }

            View.Hide();
        }
    }
}
