using System;
using Crashmania.PureMvc;
using Crashmania.PureMvc.Notifications;
using Crashmania.UI.Shell;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Crashmania.Services
{
    public sealed class NavigationService
    {
        private bool isLoading;

        public string CurrentSceneName { get; private set; }
        public string TargetTab { get; set; } = "Lobby";

        public async UniTask LoadScene(string sceneName, bool showTransition = true)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException("Scene name cannot be empty.", nameof(sceneName));
            }

            if (isLoading)
            {
                return;
            }

            isLoading = true;

            try
            {
                var overlay = TransitionOverlay.Instance;
                if (showTransition && overlay != null)
                {
                    await overlay.FadeIn(0.25f);
                }

                var operation = SceneManager.LoadSceneAsync(sceneName);
                if (operation != null)
                {
                    await operation.ToUniTask();
                }

                CurrentSceneName = sceneName;

                if (showTransition && overlay != null)
                {
                    await overlay.FadeOut(0.25f);
                }

                LobbyFacade.GetInstance().SendNotification(LobbyNotifications.SceneLoaded, sceneName == "Lobby" ? TargetTab : sceneName);
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}
