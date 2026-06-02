using System;
using System.Collections.Generic;

namespace Crashmania.Models
{
    public enum CrashGamePhase
    {
        Preparation,
        Flight,
        Crashed,
        Intermission
    }

    public enum BetPanelState
    {
        Idle,
        Pending,
        InFlight,
        Won,
        Lost
    }

    [Serializable]
    public sealed class CrashPlayerBet
    {
        public string PlayerName;
        public double BetAmount;
        public CurrencyMode Currency;
        public double Multiplier;
        public double WinAmount;
        public bool IsCashedOut;
        public bool IsLocalPlayer;
        public string PanelId;
        public double? AutoCashOutMultiplier;
    }

    [Serializable]
    public sealed class AutoplaySettings
    {
        public bool Enabled;
        public int SelectedRoundCountIndex; // 0=∞, 1=10, 2=25, 3=50, 4=100
        public int RemainingRounds = -1;    // -1 = infinite
        public double CashOutMultiplier = 1.5;

        public static readonly int[] RoundCounts = { -1, 10, 25, 50, 100 };
        public const double MinCashOutMultiplier = 1.1;
        public const double CashOutStep = 0.1;
        public const double MaxCashOutMultiplier = 100.0;

        public int GetRoundCount()
        {
            return RoundCounts[SelectedRoundCountIndex];
        }

        public void Reset()
        {
            Enabled = false;
            SelectedRoundCountIndex = 0;
            RemainingRounds = -1;
            CashOutMultiplier = 1.5;
        }
    }

    public readonly struct CrashCountdownEvent
    {
        public CrashCountdownEvent(int roundNonce, float secondsRemaining)
        {
            RoundNonce = roundNonce;
            SecondsRemaining = secondsRemaining;
        }

        public int RoundNonce { get; }
        public float SecondsRemaining { get; }
    }

    public readonly struct CrashRoundStartedEvent
    {
        public CrashRoundStartedEvent(int roundNonce, double crashPoint)
        {
            RoundNonce = roundNonce;
            CrashPoint = crashPoint;
        }

        public int RoundNonce { get; }
        public double CrashPoint { get; }
    }

    public readonly struct CrashMultiplierEvent
    {
        public CrashMultiplierEvent(int roundNonce, float elapsedSeconds, double multiplier)
        {
            RoundNonce = roundNonce;
            ElapsedSeconds = elapsedSeconds;
            Multiplier = multiplier;
        }

        public int RoundNonce { get; }
        public float ElapsedSeconds { get; }
        public double Multiplier { get; }
    }

    public readonly struct CrashRoundEndedEvent
    {
        public CrashRoundEndedEvent(int roundNonce, double crashPoint)
        {
            RoundNonce = roundNonce;
            CrashPoint = crashPoint;
        }

        public int RoundNonce { get; }
        public double CrashPoint { get; }
    }

    public readonly struct CrashBetResolution
    {
        public CrashBetResolution(string panelId, bool won, double betAmount, CurrencyMode currency, double multiplier, double payout)
        {
            PanelId = panelId;
            Won = won;
            BetAmount = betAmount;
            Currency = currency;
            Multiplier = multiplier;
            Payout = payout;
        }

        public string PanelId { get; }
        public bool Won { get; }
        public double BetAmount { get; }
        public CurrencyMode Currency { get; }
        public double Multiplier { get; }
        public double Payout { get; }
    }
}
