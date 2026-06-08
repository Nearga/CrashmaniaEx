using Crashmania.Models;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
using Crashmania.UI.Game;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;

namespace Crashmania.PureMvc.Mediators
{
    public sealed class GameMediator : Mediator
    {
        public const string Name = "GameMediator";

        private GameView View => ViewComponent as GameView;

        public GameMediator(GameView view) : base(Name, view)
        {
        }

        public override void OnRegister()
        {
            View.RewardEarned += OnRewardEarned;
            View.CurrencyLockChanged += OnCurrencyLockChanged;
            SynchronizeCurrency();
            View.NotifyCurrencyLockState();
        }

        public override void OnRemove()
        {
            View.RewardEarned -= OnRewardEarned;
            View.CurrencyLockChanged -= OnCurrencyLockChanged;
            SendNotification(LobbyNotifications.GameCurrencyLockChanged, false);
        }

        public override string[] ListNotificationInterests()
        {
            return new[] { LobbyNotifications.CurrencyModeChanged };
        }

        public override void HandleNotification(INotification notification)
        {
            if (notification.Name == LobbyNotifications.CurrencyModeChanged && !View.IsCurrencyLocked)
            {
                SynchronizeCurrency();
            }
        }

        private void OnRewardEarned(CrashRewardEvent reward)
        {
            var duration = View.PlayReward(reward);
            var balance = Facade.RetrieveProxy(BalanceProxy.Name) as BalanceProxy;
            if (balance == null)
            {
                return;
            }

            if (reward.Currency == CurrencyMode.CC)
            {
                balance.Credit(reward.Payout, 0.0);
            }
            else
            {
                balance.Credit(0.0, reward.Payout);
            }

            SendNotification(LobbyNotifications.GameBalanceAnimationRequested, duration);
        }

        private void OnCurrencyLockChanged(bool locked)
        {
            SendNotification(LobbyNotifications.GameCurrencyLockChanged, locked);
            if (!locked)
            {
                SynchronizeCurrency();
            }
        }

        private void SynchronizeCurrency()
        {
            var settings = Facade.RetrieveProxy(SettingsProxy.Name) as SettingsProxy;
            if (settings != null)
            {
                View.SetCurrencyForIdlePanels(settings.ActiveCurrency);
            }
        }
    }
}
