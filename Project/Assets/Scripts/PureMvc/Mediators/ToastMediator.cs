using Crashmania.PureMvc.Notifications;
using Crashmania.UI.Shell;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;

namespace Crashmania.PureMvc.Mediators
{
    public sealed class ToastMediator : Mediator
    {
        public const string Name = "ToastMediator";

        private ToastView View => ViewComponent as ToastView;

        public ToastMediator(ToastView view) : base(Name, view)
        {
        }

        public override string[] ListNotificationInterests()
        {
            return new[] { LobbyNotifications.ShowToast };
        }

        public override void HandleNotification(INotification notification)
        {
            if (notification.Name == LobbyNotifications.ShowToast)
            {
                if (notification.Body is string message)
                {
                    View.Show(message);
                }
            }
        }
    }
}
