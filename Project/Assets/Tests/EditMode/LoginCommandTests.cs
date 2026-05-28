using System.Collections.Generic;
using System.Threading.Tasks;
using Crashmania.Config;
using Crashmania.Models;
using Crashmania.PureMvc.Commands.Auth;
using Crashmania.Services;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Crashmania.Tests
{
    public sealed class LoginCommandTests
    {
        [Test]
        public async Task AuthenticateUsesEmailLogin()
        {
            var backend = new FakeBackend { Response = SuccessfulResponse("email@test.com") };

            var response = await LoginCommand.Authenticate(backend, new LoginCredentials
            {
                Provider = LoginProvider.Email,
                Email = "email@test.com",
                Password = "secret"
            });

            Assert.IsTrue(response.Success);
            Assert.AreEqual(1, backend.EmailLoginCalls);
            Assert.AreEqual(0, backend.GoogleLoginCalls);
        }

        [Test]
        public async Task AuthenticateUsesGoogleLogin()
        {
            var backend = new FakeBackend { Response = SuccessfulResponse("google@test.com") };

            var response = await LoginCommand.Authenticate(backend, new LoginCredentials
            {
                Provider = LoginProvider.Google,
                GoogleIdToken = "token"
            });

            Assert.IsTrue(response.Success);
            Assert.AreEqual(0, backend.EmailLoginCalls);
            Assert.AreEqual(1, backend.GoogleLoginCalls);
        }

        [Test]
        public async Task AuthenticateReturnsBackendFailure()
        {
            var backend = new FakeBackend
            {
                Response = new AuthResponse { Success = false, ErrorMessage = "Nope" }
            };

            var response = await LoginCommand.Authenticate(backend, new LoginCredentials());

            Assert.IsFalse(response.Success);
            Assert.AreEqual("Nope", response.ErrorMessage);
        }

        private static AuthResponse SuccessfulResponse(string email)
        {
            return new AuthResponse
            {
                Success = true,
                AccessToken = "access",
                RefreshToken = "refresh",
                Profile = new PlayerProfile { Email = email }
            };
        }

        private sealed class FakeBackend : IBackendService
        {
            public AuthResponse Response;
            public int EmailLoginCalls;
            public int GoogleLoginCalls;

            public UniTask<AuthResponse> Login(string email, string password)
            {
                EmailLoginCalls++;
                return UniTask.FromResult(Response);
            }

            public UniTask<AuthResponse> LoginWithGoogle(string idToken)
            {
                GoogleLoginCalls++;
                return UniTask.FromResult(Response);
            }

            public UniTask<AuthResponse> RefreshToken(string refreshToken)
            {
                return UniTask.FromResult(Response);
            }

            public UniTask<LobbyDataResponse> GetLobbyData()
            {
                return UniTask.FromResult(new LobbyDataResponse());
            }

            public UniTask<PlayerProfile> GetPlayerProfile()
            {
                return UniTask.FromResult(new PlayerProfile());
            }

            public UniTask<List<StorePackage>> GetStorePackages()
            {
                return UniTask.FromResult(new List<StorePackage>());
            }

            public UniTask<PurchaseResult> PurchasePackage(string packageId)
            {
                return UniTask.FromResult(new PurchaseResult());
            }

            public UniTask<BonusStatus> GetBonusStatus(BonusType type)
            {
                return UniTask.FromResult(new BonusStatus());
            }

            public UniTask<ClaimResult> ClaimBonus(BonusType type)
            {
                return UniTask.FromResult(new ClaimResult());
            }

            public UniTask<GameSession> StartGameSession(string gameId, string accessToken)
            {
                return UniTask.FromResult(new GameSession());
            }
        }
    }
}
