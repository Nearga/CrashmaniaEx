using Crashmania.Audio;
using Crashmania.Config;
using Crashmania.PureMvc;
using Crashmania.PureMvc.Mediators;
using Crashmania.PureMvc.Proxies;
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

            var header = Object.FindAnyObjectByType<HeaderView>(FindObjectsInactive.Include);
            var tabBar = Object.FindAnyObjectByType<TabBarView>(FindObjectsInactive.Include);
            if (tabBar != null)
            {
                tabBar.Initialize(tokens);
            }

            var modal = Object.FindAnyObjectByType<ModalView>(FindObjectsInactive.Include);
            var toast = Object.FindAnyObjectByType<ToastView>(FindObjectsInactive.Include);

            if (header != null) header.SetVisibleForScene(facade.RetrieveProxy(SettingsProxy.Name) != null ? SceneManager.GetActiveScene().name : "Login");
            if (tabBar != null) tabBar.SetVisibleForScene(facade.RetrieveProxy(SettingsProxy.Name) != null ? SceneManager.GetActiveScene().name : "Login");

            if (header != null) RecreateMediator(facade, HeaderMediator.Name, () => new HeaderMediator(header));
            if (tabBar != null) RecreateMediator(facade, TabBarMediator.Name, () => new TabBarMediator(tabBar));
            if (modal != null) RecreateMediator(facade, ModalMediator.Name, () => new ModalMediator(modal));
            if (toast != null) RecreateMediator(facade, ToastMediator.Name, () => new ToastMediator(toast));
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
