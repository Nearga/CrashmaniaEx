using Crashmania.Core;
using NUnit.Framework;

namespace Crashmania.Tests
{
    public sealed class DependencyContainerSmokeTests
    {
        [Test]
        public void RegisterResolveAndInjectString()
        {
            var container = DependencyContainer.Instance;
            container.Clear();
            container.Register<string>("phase-one");

            Assert.AreEqual("phase-one", container.Resolve<string>());

            var target = new InjectionTarget();
            container.Inject(target);

            Assert.AreEqual("phase-one", target.Value);
        }

        private sealed class InjectionTarget
        {
            [Inject] private string value;

            public string Value => value;
        }
    }
}
