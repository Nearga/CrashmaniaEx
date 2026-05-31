using System;
using Crashmania.Models;
using Crashmania.Services;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Game
{
    public sealed class BetPanelController : MonoBehaviour
    {
        [SerializeField] private string panelId = "BetPanel";
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text actionLabel;
        [SerializeField] private TMP_Text actionSubLabel;
        [SerializeField] private TMP_Text stateLabel;
        [SerializeField] private Button decrementButton;
        [SerializeField] private Button incrementButton;
        [SerializeField] private Button actionButton;
        [SerializeField] private Toggle autoplayToggle;
        [SerializeField] private Image actionBackground;
        [SerializeField] private double betAmount = 6000.0;
        [SerializeField] private double stepAmount = 1000.0;

        private ICrashGameService service;
        private CurrencyMode currency = CurrencyMode.CC;
        private BetPanelState state = BetPanelState.Idle;
        private double currentMultiplier = 1.0;

        public event Action<double, CurrencyMode> BetAccepted;
        public event Action<double, CurrencyMode> BetCancelled;

        public string PanelId => panelId;
        public BetPanelState State => state;

        private void Awake()
        {
            amountText ??= FindDeep<TMP_Text>("AmountText");
            actionLabel ??= FindDeep<TMP_Text>("ActionLabel");
            actionSubLabel ??= FindDeep<TMP_Text>("ActionSubLabel");
            stateLabel ??= FindDeep<TMP_Text>("StateLabel");
            decrementButton ??= FindDeep<Button>("MinusButton");
            incrementButton ??= FindDeep<Button>("PlusButton");
            actionButton ??= FindDeep<Button>("ActionButton");
            autoplayToggle ??= FindDeep<Toggle>("AutoplayToggle");
            actionBackground ??= actionButton != null ? actionButton.GetComponent<Image>() : null;

            if (decrementButton != null) decrementButton.onClick.AddListener(() => AdjustAmount(-stepAmount));
            if (incrementButton != null) incrementButton.onClick.AddListener(() => AdjustAmount(stepAmount));
            if (actionButton != null) actionButton.onClick.AddListener(OnActionPressed);

            BindQuickButton("Quick10K", 10000);
            BindQuickButton("Quick20K", 20000);
            BindQuickButton("Quick40K", 40000);
            BindQuickButton("Quick60K", 60000);
            BindQuickButton("Quick80K", 80000);

            Render();
        }

        public void Initialize(string id, ICrashGameService crashService, CurrencyMode activeCurrency)
        {
            panelId = id;
            service = crashService;
            currency = activeCurrency;
            SetState(BetPanelState.Idle);
        }

        public void SetCurrency(CurrencyMode activeCurrency)
        {
            currency = activeCurrency;
            Render();
        }

        public void OnCountdown()
        {
            if (state == BetPanelState.Won || state == BetPanelState.Lost)
            {
                SetState(BetPanelState.Idle);
            }
        }

        public void OnRoundStarted()
        {
            if (state == BetPanelState.Pending)
            {
                SetState(BetPanelState.InFlight);
            }
        }

        public void OnMultiplierUpdated(double multiplier)
        {
            currentMultiplier = multiplier;
            if (state == BetPanelState.InFlight)
            {
                Render();
            }
        }

        public void Resolve(CrashBetResolution resolution)
        {
            if (resolution.PanelId != panelId)
            {
                return;
            }

            SetState(resolution.Won ? BetPanelState.Won : BetPanelState.Lost);
        }

        private void OnActionPressed()
        {
            if (service == null)
            {
                return;
            }

            switch (state)
            {
                case BetPanelState.Idle:
                    if (service.PlaceBet(panelId, betAmount, currency, null))
                    {
                        BetAccepted?.Invoke(betAmount, currency);
                        SetState(BetPanelState.Pending);
                    }
                    break;
                case BetPanelState.Pending:
                    if (service.CancelBet(panelId))
                    {
                        BetCancelled?.Invoke(betAmount, currency);
                        SetState(BetPanelState.Idle);
                    }
                    break;
                case BetPanelState.InFlight:
                    service.CashOut(panelId);
                    break;
            }
        }

        private void AdjustAmount(double delta)
        {
            if (state != BetPanelState.Idle)
            {
                return;
            }

            betAmount = Math.Max(1000.0, betAmount + delta);
            Render();
        }

        private void SetAmount(double value)
        {
            if (state != BetPanelState.Idle)
            {
                return;
            }

            betAmount = Math.Max(1000.0, value);
            Render();
        }

        private void SetState(BetPanelState nextState)
        {
            state = nextState;
            Render();
        }

        private void Render()
        {
            if (amountText != null) amountText.text = FormatAmount(betAmount);
            if (stateLabel != null) stateLabel.text = state.ToString().ToUpperInvariant();

            var actionText = "BET";
            var subText = $"{currency} {FormatAmount(betAmount)}";
            var color = new Color(0.08f, 0.42f, 0.95f);

            if (state == BetPanelState.Pending)
            {
                actionText = "CANCEL BET";
                color = new Color(0.93f, 0.18f, 0.22f);
            }
            else if (state == BetPanelState.InFlight)
            {
                actionText = $"CASHOUT {FormatAmount(betAmount * currentMultiplier)}";
                subText = $"{currentMultiplier:F2}x";
                color = new Color(1.0f, 0.56f, 0.08f);
            }
            else if (state == BetPanelState.Won)
            {
                actionText = "WON";
                subText = "Next round";
                color = new Color(0.16f, 0.72f, 0.32f);
            }
            else if (state == BetPanelState.Lost)
            {
                actionText = "LOST";
                subText = "Next round";
                color = new Color(0.36f, 0.37f, 0.44f);
            }

            if (actionLabel != null) actionLabel.text = actionText;
            if (actionSubLabel != null) actionSubLabel.text = subText;
            if (actionBackground != null) actionBackground.DOColor(color, 0.15f);

            var editable = state == BetPanelState.Idle;
            if (decrementButton != null) decrementButton.interactable = editable;
            if (incrementButton != null) incrementButton.interactable = editable;
            if (autoplayToggle != null) autoplayToggle.interactable = editable;
        }

        private void BindQuickButton(string buttonName, double value)
        {
            var button = FindDeep<Button>(buttonName);
            if (button != null)
            {
                button.onClick.AddListener(() => SetAmount(value));
            }
        }

        private T FindDeep<T>(string objectName) where T : Component
        {
            var transforms = GetComponentsInChildren<Transform>(true);
            foreach (var child in transforms)
            {
                if (child.name == objectName && child.TryGetComponent(out T component))
                {
                    return component;
                }
            }

            return null;
        }

        private static string FormatAmount(double amount)
        {
            if (amount >= 1000000.0) return $"{amount / 1000000.0:0.#}M";
            if (amount >= 1000.0) return $"{amount / 1000.0:0.#}K";
            return amount.ToString("0");
        }
    }
}
