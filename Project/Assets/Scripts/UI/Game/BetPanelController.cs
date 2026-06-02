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

                [Header("Autoplay Submenu")]
        [SerializeField] private GameObject autoplaySubmenu;
        [SerializeField] private Button roundInfinityButton;
        [SerializeField] private Button round10Button;
        [SerializeField] private Button round25Button;
        [SerializeField] private Button round50Button;
        [SerializeField] private Button round100Button;
        [SerializeField] private TMP_Text cashOutMultiplierText;
        [SerializeField] private Button cashOutDecrementButton;
        [SerializeField] private Button cashOutIncrementButton;
        [SerializeField] private Image[] roundPresetBackgrounds;

        private ICrashGameService service;
        private CurrencyMode currency = CurrencyMode.CC;
        private BetPanelState state = BetPanelState.Idle;
        private double currentMultiplier = 1.0;
        private AutoplaySettings autoplay = new();

        private const float SubmenuAnimDuration = 0.15f;

        public event Action<double, CurrencyMode> BetAccepted;
        public event Action<double, CurrencyMode> BetCancelled;

        public string PanelId => panelId;
        public BetPanelState State => state;
        public AutoplaySettings Autoplay => autoplay;

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

            InitializeAutoplaySubmenu();
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

            // Auto-place bet during PREPARATION if autoplay is enabled and panel is idle
            if (autoplay.Enabled && state == BetPanelState.Idle && service != null)
            {
                var cashOut = autoplay.CashOutMultiplier;
                if (service.PlaceBet(panelId, betAmount, currency, cashOut))
                {
                    BetAccepted?.Invoke(betAmount, currency);
                    SetState(BetPanelState.Pending);
                }
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

            // Auto-cashout during FLIGHT when multiplier reaches configured threshold
            if (autoplay.Enabled && state == BetPanelState.InFlight && service != null)
            {
                if (multiplier >= autoplay.CashOutMultiplier)
                {
                    service.CashOut(panelId);
                }
            }

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

            // After round resolution, handle autoplay round counting
            if (autoplay.Enabled)
            {
                if (autoplay.RemainingRounds > 0)
                {
                    autoplay.RemainingRounds--;
                    if (autoplay.RemainingRounds == 0)
                    {
                        // Finite rounds exhausted — disable autoplay
                        autoplay.Enabled = false;
                        UpdateAutoplayToggleVisual(false);
                        SetSubmenuVisible(false);
                    }
                }
                // Infinite rounds (RemainingRounds == -1) continue until manually disabled
            }
        }

        public void ResetAutoplay()
        {
            autoplay.Reset();
            UpdateAutoplayToggleVisual(false);
            SetSubmenuVisible(false);
            Render();
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
                    if (service.PlaceBet(panelId, betAmount, currency, autoplay.Enabled ? autoplay.CashOutMultiplier : (double?)null))
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

        private void OnAutoplayToggleChanged(bool isOn)
        {
            if (isOn)
            {
                // Enable autoplay with default settings
                autoplay.Enabled = true;
                autoplay.SelectedRoundCountIndex = 0; // ∞
                autoplay.RemainingRounds = -1;
                autoplay.CashOutMultiplier = 1.5;
                SetSubmenuVisible(true);
                UpdateRoundPresetHighlight();
                RenderCashOutMultiplier();
            }
            else
            {
                DisableAutoplay();
            }
        }

        private void DisableAutoplay()
        {
            // If currently in an active bet, cancel or cash out first
            if (state == BetPanelState.Pending && service != null)
            {
                if (service.CancelBet(panelId))
                {
                    BetCancelled?.Invoke(betAmount, currency);
                    SetState(BetPanelState.Idle);
                }
            }
            else if (state == BetPanelState.InFlight && service != null)
            {
                service.CashOut(panelId);
            }

            autoplay.Enabled = false;
            SetSubmenuVisible(false);
            UpdateAutoplayToggleVisual(false);
            Render();
        }

        private void OnRoundPresetClicked(int index)
        {
            autoplay.SelectedRoundCountIndex = index;
            autoplay.RemainingRounds = AutoplaySettings.RoundCounts[index];
            autoplay.Enabled = true;
            UpdateRoundPresetHighlight();
            UpdateAutoplayToggleVisual(true);
        }

        private void OnCashOutMultiplierChanged(double delta)
        {
            autoplay.CashOutMultiplier = Math.Round(
                Math.Clamp(autoplay.CashOutMultiplier + delta, AutoplaySettings.MinCashOutMultiplier, AutoplaySettings.MaxCashOutMultiplier),
                1);
            RenderCashOutMultiplier();
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

        private void SetSubmenuVisible(bool visible)
        {
            if (autoplaySubmenu == null) return;

            if (visible)
            {
                autoplaySubmenu.SetActive(true);
                var cg = autoplaySubmenu.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.DOKill();
                    cg.DOFade(1f, SubmenuAnimDuration);
                }
                var rt = autoplaySubmenu.GetComponent<RectTransform>();
                if (rt != null)
                {
                    var scale = rt.localScale;
                    rt.localScale = new Vector3(0.95f, 0.95f, 1f);
                    rt.DOKill();
                    rt.DOScale(scale, SubmenuAnimDuration).SetEase(Ease.OutBack);
                }
            }
            else
            {
                var cg = autoplaySubmenu.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.DOKill();
                    cg.DOFade(0f, SubmenuAnimDuration).OnComplete(() => autoplaySubmenu.SetActive(false));
                }
                else
                {
                    autoplaySubmenu.SetActive(false);
                }
            }
        }

        private void UpdateAutoplayToggleVisual(bool isOn)
        {
            if (autoplayToggle != null)
            {
                autoplayToggle.SetIsOnWithoutNotify(isOn);
            }
        }

        private void UpdateRoundPresetHighlight()
        {
            if (roundPresetBackgrounds == null) return;
            var selected = autoplay.SelectedRoundCountIndex;
            for (var i = 0; i < roundPresetBackgrounds.Length; i++)
            {
                if (roundPresetBackgrounds[i] != null)
                {
                    roundPresetBackgrounds[i].color = i == selected
                        ? new Color(0.48f, 0.24f, 0.95f) // brandPurple highlight
                        : new Color(0.15f, 0.18f, 0.28f); // dark default
                }
            }
        }

        private void RenderCashOutMultiplier()
        {
            if (cashOutMultiplierText != null)
            {
                cashOutMultiplierText.text = $"{autoplay.CashOutMultiplier:F1}x";
            }
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

        /// <summary>
        /// Creates the autoplay submenu hierarchy programmatically.
        /// TODO: Phase 11.2 — migrate this to BetPanel.prefab via Unity MCP for artist editability.
        /// </summary>


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

        
        private void InitializeAutoplaySubmenu()
        {
            autoplaySubmenu ??= FindDeep<Transform>("AutoplaySubmenu")?.gameObject;
            roundInfinityButton ??= FindDeep<Button>("RoundPreset_0");
            round10Button ??= FindDeep<Button>("RoundPreset_1");
            round25Button ??= FindDeep<Button>("RoundPreset_2");
            round50Button ??= FindDeep<Button>("RoundPreset_3");
            round100Button ??= FindDeep<Button>("RoundPreset_4");
            
            var cashOutRow = FindDeep<Transform>("CashOutRow");
            if (cashOutRow != null)
            {
                cashOutDecrementButton ??= cashOutRow.Find("CashOutDecrement")?.GetComponent<Button>();
                cashOutIncrementButton ??= cashOutRow.Find("CashOutIncrement")?.GetComponent<Button>();
                cashOutMultiplierText ??= cashOutRow.Find("CashOutValue/ValueText")?.GetComponent<TMP_Text>();
            }

            if (roundPresetBackgrounds == null || roundPresetBackgrounds.Length == 0)
            {
                roundPresetBackgrounds = new Image[5];
                roundPresetBackgrounds[0] = roundInfinityButton?.GetComponent<Image>();
                roundPresetBackgrounds[1] = round10Button?.GetComponent<Image>();
                roundPresetBackgrounds[2] = round25Button?.GetComponent<Image>();
                roundPresetBackgrounds[3] = round50Button?.GetComponent<Image>();
                roundPresetBackgrounds[4] = round100Button?.GetComponent<Image>();
            }

            if (roundInfinityButton != null) roundInfinityButton.onClick.AddListener(() => OnRoundPresetClicked(0));
            if (round10Button != null) round10Button.onClick.AddListener(() => OnRoundPresetClicked(1));
            if (round25Button != null) round25Button.onClick.AddListener(() => OnRoundPresetClicked(2));
            if (round50Button != null) round50Button.onClick.AddListener(() => OnRoundPresetClicked(3));
            if (round100Button != null) round100Button.onClick.AddListener(() => OnRoundPresetClicked(4));

            if (cashOutDecrementButton != null) cashOutDecrementButton.onClick.AddListener(() => OnCashOutMultiplierChanged(-AutoplaySettings.CashOutStep));
            if (cashOutIncrementButton != null) cashOutIncrementButton.onClick.AddListener(() => OnCashOutMultiplierChanged(AutoplaySettings.CashOutStep));

            if (autoplayToggle != null)
            {
                autoplayToggle.onValueChanged.RemoveAllListeners();
                autoplayToggle.onValueChanged.AddListener(OnAutoplayToggleChanged);
            }

            if (autoplaySubmenu != null)
            {
                autoplaySubmenu.SetActive(false);
            }
        }
private static string FormatAmount(double amount)
        {
            if (amount >= 1000000.0) return $"{amount / 1000000.0:0.#}M";
            if (amount >= 1000.0) return $"{amount / 1000.0:0.#}K";
            return amount.ToString("0");
        }
    }
}