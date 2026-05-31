using System;
using System.Collections.Generic;
using Crashmania.Config;
using Crashmania.Models;
using Cysharp.Threading.Tasks;

namespace Crashmania.Services
{
    public interface ICrashGameService
    {
        CrashGamePhase CurrentPhase { get; }
        double CurrentMultiplier { get; }
        bool IsRunning { get; }

        event Action<CrashCountdownEvent> CountdownTick;
        event Action<CrashRoundStartedEvent> RoundStarted;
        event Action<CrashMultiplierEvent> MultiplierUpdated;
        event Action<CrashRoundEndedEvent> RoundEnded;
        event Action<int> IntermissionStarted;
        event Action<IReadOnlyList<CrashPlayerBet>> PlayerBetsUpdated;
        event Action<CrashBetResolution> BetResolved;

        UniTask StartLoop(AppConfig config);
        void StopLoop();
        bool PlaceBet(string panelId, double amount, CurrencyMode currency, double? autoCashOutMultiplier);
        bool CancelBet(string panelId);
        bool CashOut(string panelId);
    }
}
