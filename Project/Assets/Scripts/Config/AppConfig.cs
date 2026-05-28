using UnityEngine;

namespace Crashmania.Config
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "Lobby/App Configuration")]
    public sealed class AppConfig : ScriptableObject
    {
        [Header("Environment & Endpoint Targets (Future Live Setup)")]
        [Tooltip("Target URL for real REST APIs when mock mode is off.")]
        public string apiBaseUrl = "https://api.crashmania.com/api";
        [Tooltip("Target URL for real game WebSockets when mock mode is off.")]
        public string webSocketUrl = "wss://crash.crashmania.com/ws";

        [Header("CTO Demo Mock Settings")]
        [Tooltip("Enables self-contained client-side simulation. Must be TRUE for this demo.")]
        public bool enableOfflineMocks = true;
        [Tooltip("Virtual delay (ms) for REST HTTP API simulations.")]
        public int mockNetworkDelayMs = 350;
        [Tooltip("Instant crash house edge (e.g. 0.03 for 3%).")]
        public float houseEdgeRate = 0.03f;

        [Header("Default User Starting Ledgers")]
        public double startingBalanceCC = 250000.0;
        public double startingBalanceSC = 5.00;
        public string demoUserDisplayName = "CTO_Guest";
        public int defaultVipTier = 1;

        [Header("Hourly Bonus Preferences")]
        public double hourlyBonusAmountCC = 10000.0;
        public double hourlyBonusIntervalSeconds = 7200f;

        [Header("Daily Streak Rewards (Day 1-7 CC Values)")]
        public double[] dailyStreakCcRewards = { 10000, 15000, 20000, 25000, 30000, 40000, 50000 };
        public double dailyStreakDay7ScBonus = 1.00;
    }
}
