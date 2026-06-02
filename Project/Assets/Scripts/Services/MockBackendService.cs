using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Crashmania.Config;
using Crashmania.Game;
using Crashmania.Models;
using Cysharp.Threading.Tasks;

namespace Crashmania.Services
{
    public sealed class MockBackendService : IBackendService, ICrashGameService
    {
        private const float PreparationSeconds = 8f;
        private const float CrashedSeconds = 2.5f;
        private const float IntermissionSeconds = 1.5f;
        private const int MultiplierTickMs = 50;

        private static readonly string[] MockNames =
        {
            "alex****n", "sky_rider", "nova777", "mila_coin", "ct_dash",
            "rocket_max", "lucky_pam", "zenith9", "coinpilot", "astro_j"
        };

        private readonly AppConfig config;
        private readonly PlayerProfile profile;
        private readonly System.Random random = new(731928);
        private readonly List<CrashPlayerBet> mockBets = new();
        private readonly Dictionary<string, CrashPlayerBet> localBets = new();
        private CancellationTokenSource crashLoopCts;
        private int roundNonce;

        public MockBackendService(AppConfig config)
        {
            this.config = config;
            profile = new PlayerProfile
            {
                Id = "mock-user-001",
                Email = "player@test.com",
                DisplayName = config != null ? config.demoUserDisplayName : "CTO_Guest",
                BalanceCC = config != null ? config.startingBalanceCC : 250000.0,
                BalanceSC = config != null ? config.startingBalanceSC : 5.0,
                VipTier = config != null ? config.defaultVipTier : 1
            };
        }

        public CrashGamePhase CurrentPhase { get; private set; } = CrashGamePhase.Intermission;
        public double CurrentMultiplier { get; private set; } = 1.0;
        public bool IsRunning => crashLoopCts != null && !crashLoopCts.IsCancellationRequested;

        public event Action<CrashCountdownEvent> CountdownTick;
        public event Action<CrashRoundStartedEvent> RoundStarted;
        public event Action<CrashMultiplierEvent> MultiplierUpdated;
        public event Action<CrashRoundEndedEvent> RoundEnded;
        public event Action<int> IntermissionStarted;
        public event Action<IReadOnlyList<CrashPlayerBet>> PlayerBetsUpdated;
        public event Action<CrashBetResolution> BetResolved;

        public async UniTask<AuthResponse> Login(string email, string password)
        {
            await Delay();
            return AuthSuccess(email);
        }

        public async UniTask<AuthResponse> LoginWithGoogle(string idToken)
        {
            await Delay();
            return AuthSuccess("google-player@test.com");
        }

        public async UniTask<AuthResponse> RefreshToken(string refreshToken)
        {
            await Delay();
            return AuthSuccess(profile.Email);
        }

        public async UniTask<LobbyDataResponse> GetLobbyData()
        {
            await Delay();
            return MockCatalog.Create();
        }

        public async UniTask<PlayerProfile> GetPlayerProfile()
        {
            await Delay();
            return profile;
        }

        public async UniTask<List<StorePackage>> GetStorePackages()
        {
            await Delay();
            return new List<StorePackage>
            {
                new() { Id = "pack-1", Name = "Tiny Pack", CoinsCC = 110000, BonusSC = 2, PriceLabel = "$1.99" },
                new() { Id = "pack-2", Name = "Small Pack", CoinsCC = 250000, BonusSC = 5, PriceLabel = "$4.99" },
                new() { Id = "pack-3", Name = "Medium Pack", CoinsCC = 550000, BonusSC = 10, PriceLabel = "$9.99" },
                new() { Id = "pack-4", Name = "Large Pack", CoinsCC = 1200000, BonusSC = 20, PriceLabel = "$19.99" },
                new() { Id = "pack-5", Name = "Super Pack", CoinsCC = 3500000, BonusSC = 50, PriceLabel = "$49.99" },
                new() { Id = "pack-6", Name = "Mega Pack", CoinsCC = 7500000, BonusSC = 100, PriceLabel = "$99.99" }
            };
        }

        public async UniTask<PurchaseResult> PurchasePackage(string packageId)
        {
            await Delay();
            
            // For mock purposes, we find the package to get the amounts to credit
            var packages = await GetStorePackages();
            var package = packages.Find(p => p.Id == packageId);
            
            if (package == null) return new PurchaseResult { Success = false, ErrorMessage = "Package not found" };

            profile.BalanceCC += package.CoinsCC;
            profile.BalanceSC += package.BonusSC;

            return new PurchaseResult 
            { 
                Success = true, 
                CreditedCC = package.CoinsCC, 
                CreditedSC = package.BonusSC 
            };
        }

        public async UniTask<BonusStatus> GetBonusStatus(BonusType type)
        {
            await Delay();
            return new BonusStatus { Type = type, CanClaim = false };
        }

        public async UniTask<ClaimResult> ClaimBonus(BonusType type)
        {
            await Delay();
            return new ClaimResult { Success = true };
        }

        public async UniTask<GameSession> StartGameSession(string gameId, string accessToken)
        {
            await Delay();
            return new GameSession
            {
                SessionId = $"mock-session-{Guid.NewGuid():N}",
                GameId = gameId,
                AccessToken = accessToken
            };
        }

        public UniTask StartLoop(AppConfig loopConfig)
        {
            if (IsRunning)
            {
                return UniTask.CompletedTask;
            }

            crashLoopCts = new CancellationTokenSource();
            RunCrashLoop(loopConfig ?? config, crashLoopCts.Token).Forget();
            return UniTask.CompletedTask;
        }

        public void StopLoop()
        {
            if (crashLoopCts == null)
            {
                return;
            }

            crashLoopCts.Cancel();
            crashLoopCts = null;
            localBets.Clear();
            mockBets.Clear();
            CurrentPhase = CrashGamePhase.Intermission;
            CurrentMultiplier = 1.0;
        }

        public bool PlaceBet(string panelId, double amount, CurrencyMode currency, double? autoCashOutMultiplier)
        {
            if (CurrentPhase != CrashGamePhase.Preparation || string.IsNullOrWhiteSpace(panelId) || localBets.ContainsKey(panelId))
            {
                return false;
            }

            var bet = new CrashPlayerBet
            {
                PlayerName = "YOU",
                BetAmount = amount,
                Currency = currency,
                PanelId = panelId,
                IsLocalPlayer = true,
                AutoCashOutMultiplier = autoCashOutMultiplier
            };

            localBets[panelId] = bet;
            RaisePlayerBetsUpdated();
            return true;
        }

        public bool CancelBet(string panelId)
        {
            if (CurrentPhase != CrashGamePhase.Preparation || string.IsNullOrWhiteSpace(panelId))
            {
                return false;
            }

            var removed = localBets.Remove(panelId);
            if (removed)
            {
                RaisePlayerBetsUpdated();
            }

            return removed;
        }

        public bool CashOut(string panelId)
        {
            if (CurrentPhase != CrashGamePhase.Flight || string.IsNullOrWhiteSpace(panelId))
            {
                return false;
            }

            if (!localBets.TryGetValue(panelId, out var bet) || bet.IsCashedOut)
            {
                return false;
            }

            ResolveBet(bet, true, CurrentMultiplier);
            return true;
        }

        private UniTask Delay()
        {
            var delayMs = config != null ? config.mockNetworkDelayMs : 0;
            return UniTask.Delay(delayMs);
        }

        private AuthResponse AuthSuccess(string email)
        {
            profile.Email = email;
            return new AuthResponse
            {
                Success = true,
                AccessToken = "mock-access-token",
                RefreshToken = "mock-refresh-token",
                Profile = profile
            };
        }

        private async UniTaskVoid RunCrashLoop(AppConfig loopConfig, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    roundNonce++;
                    CurrentMultiplier = 1.0;
                    CurrentPhase = CrashGamePhase.Preparation;
                    localBets.Clear();
                    mockBets.Clear();
                    PopulateMockBets();

                    var crashPoint = CalculateCrashPoint(loopConfig, roundNonce);
                    for (var remaining = PreparationSeconds; remaining >= 0f && !token.IsCancellationRequested; remaining -= 0.5f)
                    {
                        CountdownTick?.Invoke(new CrashCountdownEvent(roundNonce, remaining));
                        RaisePlayerBetsUpdated();
                        await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);
                    }

                    CurrentPhase = CrashGamePhase.Flight;
                    RoundStarted?.Invoke(new CrashRoundStartedEvent(roundNonce, crashPoint));
                    var elapsed = 0f;
                    while (CurrentMultiplier < crashPoint && !token.IsCancellationRequested)
                    {
                        await UniTask.Delay(MultiplierTickMs, cancellationToken: token);
                        elapsed += MultiplierTickMs / 1000f;
                        CurrentMultiplier = CrashCurveEvaluator.GetMultiplierAtTime(elapsed);
                        UpdateMockCashOuts(CurrentMultiplier);
                        MultiplierUpdated?.Invoke(new CrashMultiplierEvent(roundNonce, elapsed, CurrentMultiplier));
                    }

                    CurrentPhase = CrashGamePhase.Crashed;
                    foreach (var bet in localBets.Values.Where(bet => !bet.IsCashedOut).ToList())
                    {
                        ResolveBet(bet, false, crashPoint);
                    }

                    RoundEnded?.Invoke(new CrashRoundEndedEvent(roundNonce, crashPoint));
                    RaisePlayerBetsUpdated();
                    await UniTask.Delay(TimeSpan.FromSeconds(CrashedSeconds), cancellationToken: token);

                    CurrentPhase = CrashGamePhase.Intermission;
                    IntermissionStarted?.Invoke(roundNonce);
                    await UniTask.Delay(TimeSpan.FromSeconds(IntermissionSeconds), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void PopulateMockBets()
        {
            var count = random.Next(5, 9);
            var names = MockNames.OrderBy(_ => random.Next()).Take(count).ToArray();
            foreach (var name in names)
            {
                var amount = random.Next(4, 75) * 1000.0;
                mockBets.Add(new CrashPlayerBet
                {
                    PlayerName = name,
                    BetAmount = amount,
                    Currency = CurrencyMode.CC
                });
            }
        }

        private void UpdateMockCashOuts(double multiplier)
        {
            foreach (var bet in mockBets)
            {
                if (bet.IsCashedOut)
                {
                    continue;
                }

                var target = 1.2 + (StableNameHash(bet.PlayerName) % 220) / 100.0;
                if (multiplier >= target)
                {
                    bet.IsCashedOut = true;
                    bet.Multiplier = multiplier;
                    bet.WinAmount = Math.Floor(bet.BetAmount * multiplier);
                }
            }

            // Auto-cashout local player bets when multiplier reaches their configured threshold
            foreach (var kvp in localBets)
            {
                var bet = kvp.Value;
                if (bet.IsCashedOut || !bet.AutoCashOutMultiplier.HasValue)
                {
                    continue;
                }

                if (multiplier >= bet.AutoCashOutMultiplier.Value)
                {
                    ResolveBet(bet, true, multiplier);
                }
            }

            RaisePlayerBetsUpdated();
        }

        private void ResolveBet(CrashPlayerBet bet, bool won, double multiplier)
        {
            bet.IsCashedOut = won;
            bet.Multiplier = multiplier;
            bet.WinAmount = won ? Math.Floor(bet.BetAmount * multiplier) : 0.0;
            BetResolved?.Invoke(new CrashBetResolution(
                bet.PanelId,
                won,
                bet.BetAmount,
                bet.Currency,
                multiplier,
                bet.WinAmount));
            RaisePlayerBetsUpdated();
        }

        private void RaisePlayerBetsUpdated()
        {
            var allBets = mockBets.Concat(localBets.Values).ToList();
            PlayerBetsUpdated?.Invoke(allBets);
        }

        private static double CalculateCrashPoint(AppConfig loopConfig, int nonce)
        {
            var houseEdgeRate = loopConfig != null ? loopConfig.houseEdgeRate : 0.03f;
            var houseEdgePercent = Math.Max(0, MathfLikeRoundToInt(houseEdgeRate * 100f));
            var serverSeed = $"mock-server-seed-{nonce}";
            var clientSeed = "crashmania-demo";
            var salt = $"{clientSeed}-{nonce}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(serverSeed));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(salt));
            var hex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            var first52Bits = ulong.Parse(hex[..13], NumberStyles.HexNumber);
            if (first52Bits % 100UL < (ulong)houseEdgePercent)
            {
                return 1.0;
            }

            var e = Math.Pow(2, 52);
            var raw = ((100.0 - houseEdgePercent) * e) / (e - first52Bits) / 100.0;
            return Math.Max(1.0, Math.Floor(raw * 100.0) / 100.0);
        }

        private static int MathfLikeRoundToInt(float value)
        {
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }

        private static int StableNameHash(string value)
        {
            unchecked
            {
                var hash = 17;
                for (var i = 0; i < value.Length; i++)
                {
                    hash = hash * 31 + value[i];
                }

                return hash & int.MaxValue;
            }
        }
    }
}
