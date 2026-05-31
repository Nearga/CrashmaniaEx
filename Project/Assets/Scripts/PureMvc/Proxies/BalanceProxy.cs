using Crashmania.PureMvc.Notifications;
using PureMVC.Patterns.Proxy;

namespace Crashmania.PureMvc.Proxies
{
    public sealed class BalanceProxy : Proxy
    {
        public const string Name = "BalanceProxy";

        public BalanceProxy() : base(Name)
        {
        }

        public double BalanceCC { get; private set; }
        public double BalanceSC { get; private set; }

        public void Initialize(double cc, double sc)
        {
            BalanceCC = cc;
            BalanceSC = sc;
            SendNotification(LobbyNotifications.BalanceUpdated);
        }

        public void Credit(double cc, double sc)
        {
            BalanceCC += cc;
            BalanceSC += sc;
            SendNotification(LobbyNotifications.BalanceUpdated);
        }

        public void Debit(double cc, double sc)
        {
            BalanceCC -= cc;
            BalanceSC -= sc;
            SendNotification(LobbyNotifications.BalanceUpdated);
        }
    }
}
