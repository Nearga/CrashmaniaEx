using Crashmania.PureMvc.Notifications;
using Crashmania.UI.Modals;
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

        public override void OnRegister()
        {
            if (View != null)
            {
                View.ModalShown += OnModalShown;
            }
        }

        public override void OnRemove()
        {
            if (View != null)
            {
                View.ModalShown -= OnModalShown;
            }
        }

        public override string[] ListNotificationInterests()
        {
            return new[]
            {
                LobbyNotifications.ShowModal,
                LobbyNotifications.HideModal,
                LobbyNotifications.LoginSuccess,
                LobbyNotifications.LoginFailed
            };
        }

        public override void HandleNotification(INotification notification)
        {
            if (View == null)
            {
                return;
            }

            if (notification.Name == LobbyNotifications.ShowModal)
            {
                if (notification.Body is string modalName)
                {
                    var prefab = UnityEngine.Resources.Load<UnityEngine.GameObject>($"UI/Modals/{modalName}");
                    View.Show(prefab);
                }
                else
                {
                    View.Show(notification.Body);
                }
                return;
            }

            if (notification.Name == LobbyNotifications.HideModal)
            {
                View.Hide();
                return;
            }

            if (notification.Name == LobbyNotifications.LoginSuccess)
            {
                View.Hide();
                SendNotification(LobbyNotifications.NavigateToTab, "Lobby");
                SendNotification(LobbyNotifications.ShowToast, "Successfully Logged In!");
                return;
            }

            if (notification.Name == LobbyNotifications.LoginFailed)
            {
                SendNotification(LobbyNotifications.ShowToast, notification.Body as string ?? "Login failed.");
            }
        }

        private void OnModalShown(UnityEngine.GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            var login = instance.GetComponent<LoginModalView>();
            if (login != null)
            {
                login.CloseRequested += OnCloseRequested;
                login.LoginRequested += credentials => SendNotification(LobbyNotifications.LoginRequest, credentials);
                return;
            }

            var signup = instance.GetComponent<SignupModalView>();
            if (signup != null)
            {
                signup.CloseRequested += OnCloseRequested;
                signup.PlayNowRequested += OnSignupPlayNowRequested;
                return;
            }

            var prePopup = instance.GetComponent<SignupPrePopupModalView>();
            if (prePopup != null)
            {
                prePopup.CloseRequested += OnCloseRequested;
                prePopup.ContinueRequested += OnSignupPrePopupAccepted;
            }
        }

        private void OnCloseRequested()
        {
            SendNotification(LobbyNotifications.HideModal);
        }

        private void OnSignupPrePopupAccepted()
        {
            SendNotification(LobbyNotifications.HideModal);
            SendNotification(LobbyNotifications.ShowModal, "SignupModal");
        }

        private void OnSignupPlayNowRequested()
        {
            SendNotification(LobbyNotifications.HideModal);
            SendNotification(LobbyNotifications.NavigateToTab, "Lobby");
            SendNotification(LobbyNotifications.ShowToast, "Successfully Registered!");
        }
    }
}
