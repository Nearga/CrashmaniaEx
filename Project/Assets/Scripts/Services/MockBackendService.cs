using System;
using System.Collections.Generic;
using Crashmania.Config;
using Crashmania.Models;
using Cysharp.Threading.Tasks;

namespace Crashmania.Services
{
    public sealed class MockBackendService : IBackendService
    {
        private readonly AppConfig config;
        private readonly PlayerProfile profile;

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
    }
}
