using Crashmania.PureMvc;
using Crashmania.PureMvc.Mediators;
using Crashmania.PureMvc.Notifications;
using Crashmania.UI.Login;
using NUnit.Framework;
using UnityEngine;

namespace Crashmania.Tests
{
    public sealed class SceneLoadedCommandTests
    {
        [TearDown]
        public void TearDown()
        {
            var facade = LobbyFacade.GetInstance();
            if (facade.HasMediator(LoginMediator.Name))
            {
                facade.RemoveMediator(LoginMediator.Name);
            }

            foreach (var view in Object.FindObjectsByType<LoginView>(FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(view.gameObject);
            }
        }

        [Test]
        public void SceneLoadedRegistersLoginMediatorWithoutDuplicates()
        {
            var facade = LobbyFacade.GetInstance();
            facade.Startup();
            if (facade.HasMediator(LoginMediator.Name))
            {
                facade.RemoveMediator(LoginMediator.Name);
            }

            facade.SendNotification(LobbyNotifications.SceneLoaded, "Login");
            Assert.IsTrue(facade.HasMediator(LoginMediator.Name));

            facade.SendNotification(LobbyNotifications.SceneLoaded, "Login");
            Assert.IsTrue(facade.HasMediator(LoginMediator.Name));
        }
    }
}
