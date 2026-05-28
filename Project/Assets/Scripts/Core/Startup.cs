using Crashmania.Config;
using Crashmania.Core;
using Crashmania.PureMvc;
using Crashmania.PureMvc.Notifications;
using Crashmania.Services;
using Crashmania.UI.Shell;
using DG.Tweening;
using UnityEngine;

namespace Crashmania.Core
{
    public sealed class Startup : MonoBehaviour
    {
        [SerializeField] private DesignTokens designTokens;

        private NavigationService navigationService;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            var config = Resources.Load<AppConfig>("AppConfig");
            if (config == null)
            {
                Debug.LogError("[Startup] AppConfig could not be loaded from Resources/AppConfig.");
                return;
            }

            var container = DependencyContainer.Instance;
            container.Clear();
            container.Register<AppConfig>(config);

            if (designTokens != null)
            {
                container.Register<DesignTokens>(designTokens);
            }

            container.Register<IBackendService>(new MockBackendService(config));

            DOTween.Init(recycleAllByDefault: false, useSafeMode: true, logBehaviour: LogBehaviour.ErrorsOnly);
            DOTween.defaultEaseType = Ease.OutCubic;

            navigationService = new NavigationService();
            container.Register<NavigationService>(navigationService);

            var facade = LobbyFacade.GetInstance();
            facade.Startup();
            ShellBootstrapper.EnsureShell(designTokens, config, facade);
            facade.SendNotification(LobbyNotifications.NavigateTo, "Login");
        }

        private void Update()
        {
            if (navigationService == null || !Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            if (navigationService.CurrentSceneName == "Login")
            {
                return;
            }

            LobbyFacade.GetInstance().SendNotification(LobbyNotifications.NavigateTo, "Lobby");
        }
    }
}
