using Crashmania.Models;
using Crashmania.UI.Components;
using UnityEngine;

namespace Crashmania.UI.Shell
{
    public sealed class HeaderView : MonoBehaviour
    {
        [SerializeField] private AccumulateToBalance ccBalance;
        [SerializeField] private AccumulateToBalance scBalance;

        private void Awake()
        {
            if (ccBalance == null) ccBalance = transform.Find("Safe Area/Header Bar/CC Balance/CC Value")?.GetComponent<AccumulateToBalance>();
            if (scBalance == null) scBalance = transform.Find("Safe Area/Header Bar/SC Balance/SC Value")?.GetComponent<AccumulateToBalance>();
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
        }

        private static bool IsShellScene(string sceneName)
        {
            return sceneName == "Lobby" || sceneName == "Store" || sceneName == "Gifts" || sceneName == "Account";
        }
    }
}
