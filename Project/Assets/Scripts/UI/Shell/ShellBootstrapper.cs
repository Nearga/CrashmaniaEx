using Crashmania.Audio;
using Crashmania.Config;
using Crashmania.PureMvc;
using Crashmania.PureMvc.Mediators;
using Crashmania.PureMvc.Proxies;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Crashmania.UI.Shell
{
    public static class ShellBootstrapper
    {
        public static void EnsureShell(DesignTokens tokens, AppConfig config, LobbyFacade facade)
        {
            AudioManager.Ensure();

            var transition = Object.FindAnyObjectByType<TransitionOverlay>(FindObjectsInactive.Include) ?? 
                Object.Instantiate(Resources.Load<GameObject>("UI/Prefabs/TransitionOverlay")).GetComponent<TransitionOverlay>();
            Object.DontDestroyOnLoad(transition.gameObject);

            var activeScene = SceneManager.GetActiveScene();
            var header = FindInActiveScene<HeaderView>(activeScene);
            var tabBar = FindInActiveScene<TabBarView>(activeScene);
            var modal = FindInActiveScene<ModalView>(activeScene);
            var toast = FindInActiveScene<ToastView>(activeScene);

            if (tabBar != null)
            {
                tabBar.Initialize(tokens);
            }

            var currentSceneName = activeScene.name;
            if (header != null) header.SetVisibleForScene(facade.RetrieveProxy(SettingsProxy.Name) != null ? currentSceneName : "Login");
            if (tabBar != null) tabBar.SetVisibleForScene(facade.RetrieveProxy(SettingsProxy.Name) != null ? currentSceneName : "Login");

            if (header != null) RecreateMediator(facade, HeaderMediator.Name, () => new HeaderMediator(header));
            if (tabBar != null) RecreateMediator(facade, TabBarMediator.Name, () => new TabBarMediator(tabBar));
            if (modal != null) RecreateMediator(facade, ModalMediator.Name, () => new ModalMediator(modal));
            if (toast != null) RecreateMediator(facade, ToastMediator.Name, () => new ToastMediator(toast));
        }

        private static T FindInActiveScene<T>(Scene activeScene) where T : Component
        {
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include)
                .FirstOrDefault(c => c.gameObject.scene == activeScene);
        }

        private static void RecreateMediator(LobbyFacade facade, string name, System.Func<PureMVC.Interfaces.IMediator> factory)
        {
            if (facade.HasMediator(name))
            {
                facade.RemoveMediator(name);
            }
            facade.RegisterMediator(factory());
        }
    }
}
