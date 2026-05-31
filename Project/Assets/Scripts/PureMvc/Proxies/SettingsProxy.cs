using Crashmania.Models;
using Crashmania.PureMvc.Notifications;
using PureMVC.Patterns.Proxy;

namespace Crashmania.PureMvc.Proxies
{
    public sealed class SettingsProxy : Proxy
    {
        public const string Name = "SettingsProxy";

        public SettingsProxy() : base(Name)
        {
        }

        public CurrencyMode ActiveCurrency { get; private set; } = CurrencyMode.CC;
        public bool MusicOn { get; private set; } = true;
        public bool SfxOn { get; private set; } = true;

        public void SetCurrencyMode(CurrencyMode mode)
        {
            if (ActiveCurrency == mode) return;
            ActiveCurrency = mode;
            SendNotification(LobbyNotifications.CurrencyModeChanged);
        }

        public void ToggleMusic(bool on)
        {
            MusicOn = on;
            SendNotification(LobbyNotifications.ToggleMusic, on);
        }

        public void ToggleSfx(bool on)
        {
            SfxOn = on;
            SendNotification(LobbyNotifications.ToggleSound, on);
        }
    }
}
