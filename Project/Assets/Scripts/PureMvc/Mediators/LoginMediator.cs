using Crashmania.PureMvc.Notifications;
using Crashmania.UI.Login;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using UnityEngine;

namespace Crashmania.PureMvc.Mediators
{
    public sealed class LoginMediator : Mediator
    {
        public const string Name = "LoginMediator";

        private LoginView View => ViewComponent as LoginView;

        public LoginMediator(LoginView view) : base(Name, view)
        {
        }

        public override void OnRegister()
        {
            View.LoginRequested += OnLoginRequested;
            View.SignUpRequested += OnSignUpRequested;
        }

        public override void OnRemove()
        {
            if (View == null)
            {
                return;
            }

            View.LoginRequested -= OnLoginRequested;
            View.SignUpRequested -= OnSignUpRequested;
        }

        public override string[] ListNotificationInterests()
        {
            return new string[0];
        }

        public override void HandleNotification(INotification notification)
        {
        }

        private void OnLoginRequested()
        {
            Debug.Log("[LoginMediator] Show Login Modal");
            SendNotification(LobbyNotifications.ShowModal, "LoginModal");
        }

        private void OnSignUpRequested()
        {
            Debug.Log("[LoginMediator] Show Signup Pre-Popup Warning");
            SendNotification(LobbyNotifications.ShowModal, "SignupPrePopupModal");
        }
    }
}
