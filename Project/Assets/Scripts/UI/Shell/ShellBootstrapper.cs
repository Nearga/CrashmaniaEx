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
            TransitionOverlay.Create();
            AudioManager.Ensure();

            var header = Object.FindAnyObjectByType<HeaderView>() ?? HeaderView.Create(tokens, config);
            var tabBar = Object.FindAnyObjectByType<TabBarView>() ?? TabBarView.Create(tokens);
            var modal = Object.FindAnyObjectByType<ModalView>() ?? ModalView.Create(tokens);
            header.SetVisibleForScene("Login");
            tabBar.SetVisibleForScene("Login");

            RegisterMediatorIfMissing(facade, HeaderMediator.Name, () => new HeaderMediator(header));
            RegisterMediatorIfMissing(facade, TabBarMediator.Name, () => new TabBarMediator(tabBar));
            RegisterMediatorIfMissing(facade, ModalMediator.Name, () => new ModalMediator(modal));
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
