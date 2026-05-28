using Crashmania.Models;
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
            View.SubmitRequested += OnSubmitRequested;
            View.SignUpSelected += OnSignUpSelected;
        }

        public override void OnRemove()
        {
            if (View == null)
            {
                return;
            }

            View.SubmitRequested -= OnSubmitRequested;
            View.SignUpSelected -= OnSignUpSelected;
        }

        public override string[] ListNotificationInterests()
        {
            return new[] { LobbyNotifications.LoginSuccess, LobbyNotifications.LoginFailed };
        }

        public override void HandleNotification(INotification notification)
        {
            switch (notification.Name)
            {
                case LobbyNotifications.LoginSuccess:
                    View.SetLoading(false);
                    SendNotification(LobbyNotifications.NavigateTo, "Lobby");
                    break;
                case LobbyNotifications.LoginFailed:
                    View.SetLoading(false);
                    View.ShowError(notification.Body as string ?? "Login failed.");
                    break;
            }
        }

        private void OnSubmitRequested(LoginCredentials credentials)
        {
            View.SetLoading(true);
            SendNotification(LobbyNotifications.LoginRequest, credentials);
        }

        private void OnSignUpSelected()
        {
            Debug.Log("[LoginMediator] Sign Up selected; signup flow is not implemented yet.");
        }
    }
}
