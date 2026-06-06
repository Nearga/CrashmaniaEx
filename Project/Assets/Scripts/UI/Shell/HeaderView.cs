using Crashmania.Models;
using Crashmania.UI.Components;
using UnityEngine;

namespace Crashmania.UI.Shell
{
    public sealed class HeaderView : MonoBehaviour
    {
        [SerializeField] private AccumulateToBalance ccBalance;
        [SerializeField] private AccumulateToBalance scBalance;
        [SerializeField] private GameObject ccHighlight;
        [SerializeField] private GameObject scHighlight;
        [SerializeField] private UnityEngine.UI.Button toggleButton;
        [SerializeField] private UnityEngine.UI.Button backButton;

        public event System.Action OnToggleCurrency;
        public event System.Action OnBackClicked;

        private void Awake()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(() => OnToggleCurrency?.Invoke());
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
            }
        }

        public void SetActiveCurrency(CurrencyMode mode)
        {
            if (ccBalance != null) ccBalance.transform.parent.gameObject.SetActive(mode == CurrencyMode.CC);
            if (scBalance != null) scBalance.transform.parent.gameObject.SetActive(mode == CurrencyMode.SC);
            if (ccHighlight != null) ccHighlight.SetActive(mode == CurrencyMode.CC);
            if (scHighlight != null) scHighlight.SetActive(mode == CurrencyMode.SC);
        }

        public void SetBalances(PlayerProfile profile, bool animate)
        {
            if (profile == null)
            {
                return;
            }

            SetBalances(profile.BalanceCC, profile.BalanceSC, animate);
        }

        public void SetBalances(double cc, double sc, bool animate)
        {
            if (ccBalance != null) ccBalance.SetValue(cc, animate);
            if (scBalance != null) scBalance.SetValue(sc, animate);
        }

        public void SetVisibleForScene(string sceneName)
        {
            gameObject.SetActive(IsShellScene(sceneName));
            var isGame = sceneName == "Game";
            if (backButton != null)
            {
                backButton.gameObject.SetActive(isGame);
            }
        }

        private static bool IsShellScene(string sceneName)
        {
            return sceneName == "Lobby" || sceneName == "Store" || sceneName == "Gifts"
                || sceneName == "Redeem" || sceneName == "Account" || sceneName == "Game";
        }
    }
}
