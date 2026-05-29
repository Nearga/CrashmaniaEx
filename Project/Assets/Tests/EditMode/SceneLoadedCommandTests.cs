using Crashmania.PureMvc;
using Crashmania.PureMvc.Mediators;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Scenes;
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

            if (facade.HasMediator(LobbyMediator.Name))
            {
                facade.RemoveMediator(LobbyMediator.Name);
            }

            foreach (var controller in Object.FindObjectsByType<LoginSceneController>(FindObjectsInactive.Include))
            {
                Object.DestroyImmediate(controller.gameObject);
            }

            foreach (var view in Object.FindObjectsByType<LoginView>(FindObjectsInactive.Include))
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

            var sceneRoot = new GameObject("LoginCanvas");
            sceneRoot.AddComponent<LoginView>();
            sceneRoot.AddComponent<LoginSceneController>();

            facade.SendNotification(LobbyNotifications.SceneLoaded, "Login");
            Assert.IsTrue(facade.HasMediator(LoginMediator.Name));

            facade.SendNotification(LobbyNotifications.SceneLoaded, "Login");
            Assert.IsTrue(facade.HasMediator(LoginMediator.Name));
        }
    }
}
