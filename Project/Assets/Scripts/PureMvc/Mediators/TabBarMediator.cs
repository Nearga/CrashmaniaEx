using Crashmania.PureMvc.Notifications;
using Crashmania.UI.Shell;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;

namespace Crashmania.PureMvc.Mediators
{
    public sealed class TabBarMediator : Mediator
    {
        public const string Name = "TabBarMediator";

        private TabBarView View => ViewComponent as TabBarView;

        public TabBarMediator(TabBarView view) : base(Name, view)
        {
        }

        public override void OnRegister()
        {
            if (View != null)
            {
                View.TabSelected += OnTabSelected;
            }
        }

        public override void OnRemove()
        {
            if (View != null)
            {
                View.TabSelected -= OnTabSelected;
            }
        }

        public override string[] ListNotificationInterests()
        {
            return new[] { LobbyNotifications.SceneLoaded };
        }

        public override void HandleNotification(INotification notification)
        {
            if (View == null)
            {
                return;
            }

            var sceneName = notification.Body as string;
            View.SetVisibleForScene(sceneName);
            View.Highlight(sceneName);
        }

        private void OnTabSelected(string sceneName)
        {
            SendNotification(LobbyNotifications.NavigateTo, sceneName);
        }
    }
}
