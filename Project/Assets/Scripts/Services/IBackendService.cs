using System.Collections.Generic;
using Crashmania.Models;
using Cysharp.Threading.Tasks;

namespace Crashmania.Services
{
    public interface IBackendService
    {
        UniTask<AuthResponse> Login(string email, string password);
        UniTask<AuthResponse> LoginWithGoogle(string idToken);
        UniTask<AuthResponse> RefreshToken(string refreshToken);
        UniTask<LobbyDataResponse> GetLobbyData();
        UniTask<PlayerProfile> GetPlayerProfile();
        UniTask<List<StorePackage>> GetStorePackages();
        UniTask<PurchaseResult> PurchasePackage(string packageId);
        UniTask<BonusStatus> GetBonusStatus(BonusType type);
        UniTask<ClaimResult> ClaimBonus(BonusType type);
        UniTask<GameSession> StartGameSession(string gameId, string accessToken);
    }
}
