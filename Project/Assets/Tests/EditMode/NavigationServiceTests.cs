using System;
using System.Threading.Tasks;
using Crashmania.Services;
using NUnit.Framework;

namespace Crashmania.Tests
{
    public sealed class NavigationServiceTests
    {
        [Test]
        public async Task LoadSceneRejectsEmptySceneName()
        {
            var service = new NavigationService();
            try
            {
                await service.LoadScene(string.Empty, showTransition: false);
                Assert.Fail("Expected an ArgumentException for an empty scene name.");
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
        }
    }
}
