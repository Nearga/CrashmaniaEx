using Crashmania.Models;
using Crashmania.PureMvc.Proxies;
using NUnit.Framework;

namespace Crashmania.Tests
{
    public sealed class AuthProxyTests
    {
        [Test]
        public void SetAuthenticatedStoresAuthState()
        {
            var proxy = new AuthProxy();
            var profile = new PlayerProfile { Id = "user-1", Email = "player@test.com" };

            proxy.SetAuthenticated(new AuthResponse
            {
                Success = true,
                AccessToken = "access",
                RefreshToken = "refresh",
                Profile = profile
            });

            Assert.IsTrue(proxy.IsAuthenticated);
            Assert.IsTrue(proxy.IsFirstLogin);
            Assert.AreEqual("access", proxy.AccessToken);
            Assert.AreSame(profile, proxy.Profile);
        }

        [Test]
        public void ClearResetsAuthState()
        {
            var proxy = new AuthProxy();
            proxy.SetAuthenticated(new AuthResponse
            {
                Success = true,
                AccessToken = "access",
                RefreshToken = "refresh",
                Profile = new PlayerProfile()
            });

            proxy.Clear();

            Assert.IsFalse(proxy.IsAuthenticated);
            Assert.IsNull(proxy.AccessToken);
            Assert.IsNull(proxy.RefreshToken);
            Assert.IsNull(proxy.Profile);
            Assert.IsTrue(proxy.IsFirstLogin);
        }
    }
}
