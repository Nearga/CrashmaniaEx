using Crashmania.Config;
using Crashmania.Core;
using Crashmania.PureMvc;
using Crashmania.PureMvc.Notifications;
using Crashmania.Services;
using DG.Tweening;
using UnityEngine;

namespace Crashmania.Core
{
    public sealed class Startup : MonoBehaviour
    {
        [SerializeField] private DesignTokens designTokens;

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

            var facade = LobbyFacade.GetInstance();
            facade.Startup();
            facade.SendNotification(LobbyNotifications.NavigateTo, "Login");
        }
    }
}
