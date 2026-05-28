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
            return new LobbyDataResponse();
        }

        public async UniTask<PlayerProfile> GetPlayerProfile()
        {
            await Delay();
            return profile;
        }

        public async UniTask<List<StorePackage>> GetStorePackages()
        {
            await Delay();
            return new List<StorePackage>();
        }

        public async UniTask<PurchaseResult> PurchasePackage(string packageId)
        {
            await Delay();
            return new PurchaseResult { Success = true };
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
