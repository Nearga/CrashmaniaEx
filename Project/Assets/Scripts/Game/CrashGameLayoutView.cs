using Crashmania.UI.Components;
using Crashmania.UI.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Game
{
    public sealed class CrashGameLayoutView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button currencyToggleButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text ccBalanceText;
        [SerializeField] private TMP_Text scBalanceText;

        [Header("Flight")]
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private RectTransform rocketTransform;
        [SerializeField] private ParticleSystem flameParticles;
        [SerializeField] private GameObject explosionObject;
        [SerializeField] private ScrollingGridBackground scrollingGrid;
        [SerializeField] private CrashRocketAnimator rocketAnimator;
        [SerializeField] private CrashBackgroundAnimator backgroundAnimator;

        [Header("Lists")]
        [SerializeField] private RectTransform historyContent;
        [SerializeField] private RectTransform playerRowsContent;

        [Header("Bets")]
        [SerializeField] private BetPanelController[] betPanels;

        public Button BackButton => backButton;
        public Button CurrencyToggleButton => currencyToggleButton;
        public TMP_Text TitleText => titleText;
        public TMP_Text CcBalanceText => ccBalanceText;
        public TMP_Text ScBalanceText => scBalanceText;
        public TMP_Text MultiplierText => multiplierText;
        public TMP_Text StatusText => statusText;
        public RectTransform RocketTransform => rocketTransform;
        public ParticleSystem FlameParticles => flameParticles;
        public GameObject ExplosionObject => explosionObject;
        public ScrollingGridBackground ScrollingGrid => scrollingGrid;
        public CrashRocketAnimator RocketAnimator => rocketAnimator;
        public CrashBackgroundAnimator BackgroundAnimator => backgroundAnimator;
        public RectTransform HistoryContent => historyContent;
        public RectTransform PlayerRowsContent => playerRowsContent;
        public BetPanelController[] BetPanels => betPanels;

        private void Awake()
        {
            ResolveReferences();
        }

        public void ResolveReferences()
        {
            backButton ??= FindDeep<Button>("BackButton");
            currencyToggleButton ??= FindDeep<Button>("CurrencyToggleButton");
            titleText ??= FindDeep<TMP_Text>("GameTitle");
            ccBalanceText ??= FindDeep<TMP_Text>("CCBalanceText");
            scBalanceText ??= FindDeep<TMP_Text>("SCBalanceText");
            multiplierText ??= FindDeep<TMP_Text>("MultiplierText");
            statusText ??= FindDeep<TMP_Text>("StatusText");
            rocketTransform ??= FindDeep<RectTransform>("Rocket");
            flameParticles ??= FindDeep<ParticleSystem>("FlameParticles");
            scrollingGrid ??= FindDeep<ScrollingGridBackground>("GridBackground");
            rocketAnimator ??= GetComponentInChildren<CrashRocketAnimator>(true);
            backgroundAnimator ??= GetComponentInChildren<CrashBackgroundAnimator>(true);
            historyContent ??= FindDeep<RectTransform>("HistoryContent");
            playerRowsContent ??= FindDeep<RectTransform>("PlayerRowsContent");

            if (explosionObject == null)
            {
                var explosion = FindDeep<Transform>("Explosion");
                explosionObject = explosion != null ? explosion.gameObject : null;
            }

            if (betPanels == null || betPanels.Length == 0)
            {
                betPanels = GetComponentsInChildren<BetPanelController>(true);
            }
        }

        private T FindDeep<T>(string objectName) where T : Component
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == objectName && child.TryGetComponent(out T component))
                {
                    return component;
                }
            }

            return null;
        }
    }
}
