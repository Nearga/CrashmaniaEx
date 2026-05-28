using System.Collections.Generic;

namespace Crashmania.Models
{
    public sealed class StorePackage
    {
        public string Id;
        public string Name;
        public double CoinsCC;
        public double BonusSC;
        public string PriceLabel;
    }

    public sealed class PurchaseResult
    {
        public bool Success;
        public double CreditedCC;
        public double CreditedSC;
        public string ErrorMessage;
    }

    public enum BonusType
    {
        Hourly,
        Daily
    }

    public sealed class BonusStatus
    {
        public BonusType Type;
        public bool CanClaim;
        public double SecondsRemaining;
        public List<double> DailyRewards = new();
    }

    public sealed class ClaimResult
    {
        public bool Success;
        public double RewardCC;
        public double RewardSC;
        public string ErrorMessage;
    }

    public sealed class GameSession
    {
        public string SessionId;
        public string GameId;
        public string AccessToken;
    }
}
