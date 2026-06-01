using System.Linq;
using Crashmania.Config;
using Crashmania.Boot;
using Crashmania.Core;
using Crashmania.PureMvc;
using Crashmania.PureMvc.Notifications;
using Crashmania.Services;
using Crashmania.UI.Components;
using Crashmania.UI.Shell;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Crashmania.Core
{
    public sealed class Startup : MonoBehaviour
    {
        [SerializeField] private DesignTokens designTokens;

        private NavigationService navigationService;
        private DevSceneLoader sceneLoader;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = CanvasResolutionPolicy.TargetFrameRate;

            DontDestroyOnLoad(gameObject);
            DisableDuplicateSceneServices();

            var config = Resources.Load<AppConfig>("AppConfig");
            if (config == null)
            {
                Debug.LogError("[Startup] AppConfig could not be loaded from Resources/AppConfig.");
                return;
            }

            sceneLoader = GetComponent<DevSceneLoader>();

            var container = DependencyContainer.Instance;
            container.Clear();
            container.Register<AppConfig>(config);

            if (designTokens != null)
            {
                container.Register<DesignTokens>(designTokens);
            }

            var backendService = CreateBackendService(config);
            container.Register<IBackendService>(backendService);
            if (backendService is ICrashGameService crashGameService)
            {
                container.Register<ICrashGameService>(crashGameService);
            }

            container.Register<IGameLoader>(new EmbeddedGameLoader());

            DOTween.Init(recycleAllByDefault: false, useSafeMode: true, logBehaviour: LogBehaviour.ErrorsOnly);
            DOTween.defaultEaseType = Ease.OutCubic;

            navigationService = new NavigationService();
            container.Register<NavigationService>(navigationService);

            var facade = LobbyFacade.GetInstance();
            facade.Startup();
            ShellBootstrapper.EnsureShell(designTokens, config, facade);
            var initialScene = GetInitialSceneName();
            if (initialScene == "Lobby" || initialScene == "Store" || initialScene == "Gifts" || initialScene == "Account")
            {
                facade.SendNotification(LobbyNotifications.NavigateToTab, initialScene);
            }
            else
            {
                facade.SendNotification(LobbyNotifications.NavigateToScene, initialScene);
            }
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

            LobbyFacade.GetInstance().SendNotification(LobbyNotifications.NavigateToTab, "Lobby");
        }

        private static void DisableDuplicateSceneServices()
        {
            var activeScene = SceneManager.GetActiveScene();

            foreach (var eventSystem in FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude))
            {
                if (eventSystem.gameObject.scene != activeScene)
                {
                    eventSystem.gameObject.SetActive(false);
                }
            }

            var activeListener = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude)
                .FirstOrDefault(listener => listener.gameObject.scene == activeScene);
            if (activeListener == null)
            {
                activeListener = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude).FirstOrDefault();
            }

            foreach (var listener in FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude))
            {
                if (listener != activeListener)
                {
                    listener.enabled = false;
                }
            }
        }

        private IBackendService CreateBackendService(AppConfig config)
        {
            return sceneLoader != null
                ? sceneLoader.CreateBackendService(config)
                : new MockBackendService(config);
        }

        private string GetInitialSceneName()
        {
            return sceneLoader != null ? sceneLoader.TargetScene : "Login";
        }
    }
}
