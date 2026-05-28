using Crashmania.Audio;
using Crashmania.Config;
using Crashmania.PureMvc;
using Crashmania.PureMvc.Mediators;
using UnityEngine;

namespace Crashmania.UI.Shell
{
    public static class ShellBootstrapper
    {
        public static void EnsureShell(DesignTokens tokens, AppConfig config, LobbyFacade facade)
        {
            AudioManager.Ensure();

            var transition = Object.FindAnyObjectByType<TransitionOverlay>(FindObjectsInactive.Include) ?? 
                Object.Instantiate(Resources.Load<GameObject>("UI/Prefabs/TransitionOverlay")).GetComponent<TransitionOverlay>();

            var header = Object.FindAnyObjectByType<HeaderView>(FindObjectsInactive.Include) ?? 
                Object.Instantiate(Resources.Load<GameObject>("UI/Prefabs/HeaderOverlay")).GetComponent<HeaderView>();
                
            var tabBar = Object.FindAnyObjectByType<TabBarView>(FindObjectsInactive.Include) ?? 
                Object.Instantiate(Resources.Load<GameObject>("UI/Prefabs/TabBarOverlay")).GetComponent<TabBarView>();
            tabBar.Initialize(tokens);
                
            var modal = Object.FindAnyObjectByType<ModalView>(FindObjectsInactive.Include) ?? 
                Object.Instantiate(Resources.Load<GameObject>("UI/Prefabs/ModalManagerOverlay")).GetComponent<ModalView>();
                
            var toast = Object.FindAnyObjectByType<ToastView>(FindObjectsInactive.Include) ?? 
                Object.Instantiate(Resources.Load<GameObject>("UI/Prefabs/ToastOverlay")).GetComponent<ToastView>();
            
            Object.DontDestroyOnLoad(transition.gameObject);
            Object.DontDestroyOnLoad(header.gameObject);
            Object.DontDestroyOnLoad(tabBar.gameObject);
            Object.DontDestroyOnLoad(modal.gameObject);
            Object.DontDestroyOnLoad(toast.gameObject);

            header.SetVisibleForScene("Login");
            tabBar.SetVisibleForScene("Login");

            RegisterMediatorIfMissing(facade, HeaderMediator.Name, () => new HeaderMediator(header));
            RegisterMediatorIfMissing(facade, TabBarMediator.Name, () => new TabBarMediator(tabBar));
            RegisterMediatorIfMissing(facade, ModalMediator.Name, () => new ModalMediator(modal));
            RegisterMediatorIfMissing(facade, ToastMediator.Name, () => new ToastMediator(toast));
        }

        private static void RegisterMediatorIfMissing(LobbyFacade facade, string name, System.Func<PureMVC.Interfaces.IMediator> factory)
        {
            if (facade.HasMediator(name))
            {
                return;
            }

            facade.RegisterMediator(factory());
        }
    }
}
