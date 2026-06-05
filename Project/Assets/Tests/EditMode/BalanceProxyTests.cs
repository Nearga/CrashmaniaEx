using Crashmania.PureMvc.Proxies;
using NUnit.Framework;

namespace Crashmania.Tests
{
    public sealed class BalanceProxyTests
    {
        [Test]
        public void InitializeMarksBalanceAsInitialized()
        {
            var proxy = new BalanceProxy();

            proxy.Initialize(250000.0, 5.0);

            Assert.IsTrue(proxy.IsInitialized);
            Assert.AreEqual(250000.0, proxy.BalanceCC);
            Assert.AreEqual(5.0, proxy.BalanceSC);
        }
    }
}
