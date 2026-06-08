using System;
using System.Linq;
using Crashmania.Game;
using Crashmania.Models;
using Crashmania.UI.Components;
using Crashmania.UI.Shell;
using UnityEngine;

namespace Crashmania.UI.Game
{
    public sealed class GameView : MonoBehaviour
    {
        [SerializeField] private CrashGameController gameController;
        [SerializeField] private BetPanelController[] betPanels;
        [SerializeField] private HeaderView header;
        [SerializeField] private CurrencyRewardFlyout rewardFlyout;

        public event Action<CrashRewardEvent> RewardEarned;
        public event Action<bool> CurrencyLockChanged;

        public bool IsCurrencyLocked => betPanels != null && betPanels.Any(panel => panel != null && panel.BlocksCurrencyToggle);

        private void OnEnable()
        {
            if (gameController != null)
            {
                gameController.RewardEarned += OnRewardEarned;
            }

            if (betPanels == null)
            {
                return;
            }

            foreach (var panel in betPanels)
            {
                if (panel != null)
                {
                    panel.StateChanged += OnPanelStateChanged;
                }
            }
        }

        private void OnDisable()
        {
            if (gameController != null)
            {
                gameController.RewardEarned -= OnRewardEarned;
            }

            if (betPanels == null)
            {
                return;
            }

            foreach (var panel in betPanels)
            {
                if (panel != null)
                {
                    panel.StateChanged -= OnPanelStateChanged;
                }
            }
        }

        public void SetCurrencyForIdlePanels(CurrencyMode currency)
        {
            if (betPanels == null)
            {
                return;
            }

            foreach (var panel in betPanels)
            {
                if (panel != null && panel.State == BetPanelState.Idle)
                {
                    panel.SetCurrency(currency);
                }
            }
        }

        public float PlayReward(CrashRewardEvent reward)
        {
            var target = header != null ? header.GetRewardTarget(reward.Currency) : null;
            return rewardFlyout != null
                ? rewardFlyout.Play(reward.Currency, reward.Payout, reward.Multiplier, reward.Source, target)
                : 0f;
        }

        public void NotifyCurrencyLockState()
        {
            CurrencyLockChanged?.Invoke(IsCurrencyLocked);
        }

        private void OnRewardEarned(CrashRewardEvent reward)
        {
            RewardEarned?.Invoke(reward);
        }

        private void OnPanelStateChanged(BetPanelController panel)
        {
            CurrencyLockChanged?.Invoke(IsCurrencyLocked);
        }
    }
}
