using System;
using Crashmania.Models;
using Crashmania.PureMvc.Proxies;

namespace Crashmania.Game
{
    public interface IGameController
    {
        event Action<double, double> OnBalanceChanged;

        void Initialize(GameSession session, SettingsProxy settings);
        void OnBalanceUpdated(double newCC, double newSC);
        void OnSettingsChanged(bool musicOn, bool sfxOn);
        void Shutdown();
    }
}
