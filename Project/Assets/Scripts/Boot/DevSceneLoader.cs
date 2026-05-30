using Crashmania.Config;
using Crashmania.Services;
using UnityEngine;

namespace Crashmania.Boot
{
    public sealed class DevSceneLoader : MonoBehaviour
    {
        private const string DefaultScene = "Login";

#if UNITY_EDITOR
        [SerializeField] private string targetScene = DefaultScene;
        [SerializeField] private bool useMock = true;
#endif

        public string TargetScene
        {
            get
            {
#if UNITY_EDITOR
                return string.IsNullOrWhiteSpace(targetScene) ? DefaultScene : targetScene.Trim();
#else
                return DefaultScene;
#endif
            }
        }

        public IBackendService CreateBackendService(AppConfig config)
        {
#if UNITY_EDITOR
            if (!useMock)
            {
                Debug.LogWarning("[DevSceneLoader] Real backend is not implemented yet; using MockBackendService.");
            }
#endif

            return new MockBackendService(config);
        }
    }
}
