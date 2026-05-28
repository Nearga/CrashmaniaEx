using Crashmania.Models;
using Crashmania.PureMvc.Notifications;
using Crashmania.UI.Shell;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;

namespace Crashmania.PureMvc.Mediators
{
    public sealed class HeaderMediator : Mediator
    {
        public const string Name = "HeaderMediator";

        private HeaderView View => ViewComponent as HeaderView;

        public HeaderMediator(HeaderView view) : base(Name, view)
        {
        }

        public override string[] ListNotificationInterests()
        {
            return new[] { LobbyNotifications.BalanceUpdated, LobbyNotifications.SceneLoaded };
        }

        public override void HandleNotification(INotification notification)
        {
            switch (notification.Name)
            {
                case LobbyNotifications.BalanceUpdated:
                    if (notification.Body is PlayerProfile profile)
                    {
                        View.SetBalances(profile, animate: true);
                    }

                    break;
                case LobbyNotifications.SceneLoaded:
                    View.SetVisibleForScene(notification.Body as string);
                    break;
            }
        }
    }
}
