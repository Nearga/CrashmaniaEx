using Crashmania.Core;
using Crashmania.Models;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
using Crashmania.Services;
using Cysharp.Threading.Tasks;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

namespace Crashmania.PureMvc.Commands.Auth
{
    public sealed class LoginCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            var credentials = notification.Body as LoginCredentials ?? new LoginCredentials();
            Login(credentials).Forget();
        }

        public static UniTask<AuthResponse> Authenticate(IBackendService backend, LoginCredentials credentials)
        {
            credentials ??= new LoginCredentials();
            return credentials.Provider == LoginProvider.Google
                ? backend.LoginWithGoogle(credentials.GoogleIdToken ?? string.Empty)
                : backend.Login(credentials.Email ?? string.Empty, credentials.Password ?? string.Empty);
        }

        private async UniTaskVoid Login(LoginCredentials credentials)
        {
            try
            {
                var backend = ServiceLocator.Resolve<IBackendService>();
                var response = await Authenticate(backend, credentials);
                if (response == null || !response.Success)
                {
                    SendNotification(LobbyNotifications.LoginFailed, response != null ? response.ErrorMessage : "Login failed.");
                    return;
                }

                var authProxy = Facade.RetrieveProxy(AuthProxy.Name) as AuthProxy;
                authProxy?.SetAuthenticated(response);

                SendNotification(LobbyNotifications.BalanceUpdated, response.Profile);
                SendNotification(LobbyNotifications.LoginSuccess, response.Profile);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[LoginCommand] Login failed: {exception}");
                SendNotification(LobbyNotifications.LoginFailed, exception.Message);
            }
        }
    }
}
