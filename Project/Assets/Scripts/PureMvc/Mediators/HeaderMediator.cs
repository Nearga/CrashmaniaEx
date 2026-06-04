using Crashmania.Models;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
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

        public override void OnRegister()
        {
            View.OnToggleCurrency += OnToggleCurrency;
            View.OnBackClicked += OnBackClicked;
            
            var activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            View.SetVisibleForScene(activeSceneName);

            UpdateBalances(false);
            UpdateCurrencyMode();
        }

        public override void OnRemove()
        {
            View.OnToggleCurrency -= OnToggleCurrency;
            View.OnBackClicked -= OnBackClicked;
        }

        private void OnBackClicked()
        {
            Facade.SendNotification(LobbyNotifications.ExitGame);
        }

        public override string[] ListNotificationInterests()
        {
            return new[]
            {
                LobbyNotifications.BalanceUpdated,
                LobbyNotifications.CurrencyModeChanged,
                LobbyNotifications.SceneLoaded
            };
        }

        public override void HandleNotification(INotification notification)
        {
            if (View == null)
            {
                return;
            }

            switch (notification.Name)
            {
                case LobbyNotifications.BalanceUpdated:
                    UpdateBalances(true);
                    break;
                case LobbyNotifications.CurrencyModeChanged:
                    UpdateCurrencyMode();
                    break;
                case LobbyNotifications.SceneLoaded:
                    View.SetVisibleForScene(notification.Body as string);
                    break;
            }
        }

        private void OnToggleCurrency()
        {
            var settings = Facade.RetrieveProxy(SettingsProxy.Name) as SettingsProxy;
            if (settings == null) return;

            var nextMode = settings.ActiveCurrency == CurrencyMode.CC ? CurrencyMode.SC : CurrencyMode.CC;
            settings.SetCurrencyMode(nextMode);
        }

        private void UpdateBalances(bool animate)
        {
            var balanceProxy = Facade.RetrieveProxy(BalanceProxy.Name) as BalanceProxy;
            if (balanceProxy != null)
            {
                View.SetBalances(balanceProxy.BalanceCC, balanceProxy.BalanceSC, animate);
            }
        }

        private void UpdateCurrencyMode()
        {
            var settings = Facade.RetrieveProxy(SettingsProxy.Name) as SettingsProxy;
            if (settings != null)
            {
                View.SetActiveCurrency(settings.ActiveCurrency);
            }
        }
    }
}
