using System;
using System.Collections.Generic;
using Crashmania.Config;
using Crashmania.Core;
using Crashmania.Models;
using Crashmania.PureMvc.Proxies;
using Crashmania.Services;
using Crashmania.UI.Components;
using Crashmania.UI.Game;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Game
{
    public sealed class CrashGameController : MonoBehaviour, IGameController
    {
        [Header("Orientation Layouts")]
        [SerializeField] private DynamicOrientationManager orientationManager;
        [SerializeField] private CrashGameLayoutView portraitLayout;
        [SerializeField] private CrashGameLayoutView landscapeLayout;

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

        [Header("Animation Fidelity")]
        [SerializeField] private CrashRocketAnimator rocketAnimator;
        [SerializeField] private CrashBackgroundAnimator backgroundAnimator;

        [Header("Lists")]
        [SerializeField] private RectTransform historyContent;
        [SerializeField] private GameObject historyBadgePrefab;
        [SerializeField] private RectTransform playerRowsContent;
        [SerializeField] private GameObject playerRowPrefab;

        [Header("Bets")]
        [SerializeField] private BetPanelController[] betPanels;

        private readonly List<GameObject> playerRows = new();
        private readonly List<double> historyPoints = new();
        private readonly List<CrashPlayerBet> latestPlayerBets = new();
        private ICrashGameService service;
        private SettingsProxy settings;
        private CrashGameLayoutView activeLayout;
        private double balanceCc;
        private double balanceSc;
        private string currentTitle = "CRASH";
        private string currentStatus = "WAITING";
        private string currentMultiplierText = "1.00x";
        private Color currentMultiplierColor = Color.white;
        private CrashGamePhase currentPhase = CrashGamePhase.Preparation;
        private float lastCountdownSeconds = 8f;
        private CrashMultiplierEvent lastMultiplierUpdate = new(0, 0f, 1.0);
        private double lastCrashPoint = 1.0;
        private float canvasHeight;
        private const float DesignHeight = 2532f;
        private Canvas cachedRootCanvas;

        public event Action<double, double> OnBalanceChanged;
        public event Action OnRequestExit;

        private void Awake()
        {
            ResolveLayouts();
            if (orientationManager != null)
            {
                orientationManager.OrientationChanged += OnOrientationChanged;
            }

            BindActiveLayout(ResolveActiveLayout(), false);
            RefreshCanvasHeight();
        }

        private void Update()
        {
            RefreshCanvasHeight();
        }

        private void RefreshCanvasHeight()
        {
            if (cachedRootCanvas == null)
            {
                cachedRootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
            }

            if (cachedRootCanvas != null)
            {
                var rect = cachedRootCanvas.GetComponent<RectTransform>();
                if (rect != null && rect.rect.height > 0f)
                {
                    canvasHeight = rect.rect.height;
                }
            }
        }

        private float SY(float designY)
        {
            return designY * (canvasHeight / DesignHeight);
        }

        private void OnDestroy()
        {
            if (orientationManager != null)
            {
                orientationManager.OrientationChanged -= OnOrientationChanged;
            }

            UnbindActiveControls();
        }

        public void Initialize(GameSession session, SettingsProxy settingsProxy)
        {
            settings = settingsProxy;
            service = ServiceLocator.Resolve<ICrashGameService>();
            var config = ServiceLocator.Resolve<AppConfig>();

            currentTitle = string.IsNullOrWhiteSpace(session?.GameId) ? "CRASH" : session.GameId.ToUpperInvariant();
            BindActiveLayout(ResolveActiveLayout(), false);
            RenderCurrentState();

            if (service != null)
            {
                Subscribe(service);
                service.StartLoop(config).Forget();
            }

            InitializeActiveBetPanels(null);
            ResetFlightVisuals();
        }

        public void OnBalanceUpdated(double newCC, double newSC)
        {
            balanceCc = newCC;
            balanceSc = newSC;
            RenderBalance();
        }

        public void OnSettingsChanged(bool musicOn, bool sfxOn)
        {
        }

        public void Shutdown()
        {
            if (service != null)
            {
                Unsubscribe(service);
                service.StopLoop();
                service = null;
            }

            UnbindActiveControls();
        }

        private void Subscribe(ICrashGameService crashService)
        {
            crashService.CountdownTick += OnCountdownTick;
            crashService.RoundStarted += OnRoundStarted;
            crashService.MultiplierUpdated += OnMultiplierUpdated;
            crashService.RoundEnded += OnRoundEnded;
            crashService.IntermissionStarted += OnIntermissionStarted;
            crashService.PlayerBetsUpdated += RenderPlayers;
            crashService.BetResolved += OnBetResolved;
        }

        private void Unsubscribe(ICrashGameService crashService)
        {
            crashService.CountdownTick -= OnCountdownTick;
            crashService.RoundStarted -= OnRoundStarted;
            crashService.MultiplierUpdated -= OnMultiplierUpdated;
            crashService.RoundEnded -= OnRoundEnded;
            crashService.IntermissionStarted -= OnIntermissionStarted;
            crashService.PlayerBetsUpdated -= RenderPlayers;
            crashService.BetResolved -= OnBetResolved;
        }

        private void OnOrientationChanged(bool isPortrait)
        {
            var snapshots = CapturePanelSnapshots();
            BindActiveLayout(ResolveActiveLayout(), false);
            InitializeActiveBetPanels(snapshots);
            RenderCurrentState();
        }

        private void BindActiveLayout(CrashGameLayoutView layout, bool preservePanelState)
        {
            var snapshots = preservePanelState ? CapturePanelSnapshots() : null;
            UnbindActiveControls();
            activeLayout = layout;

            if (activeLayout != null)
            {
                activeLayout.ResolveReferences();
                backButton = activeLayout.BackButton;
                currencyToggleButton = activeLayout.CurrencyToggleButton;
                titleText = activeLayout.TitleText;
                ccBalanceText = activeLayout.CcBalanceText;
                scBalanceText = activeLayout.ScBalanceText;
                multiplierText = activeLayout.MultiplierText;
                statusText = activeLayout.StatusText;
                rocketTransform = activeLayout.RocketTransform;
                flameParticles = activeLayout.FlameParticles;
                explosionObject = activeLayout.ExplosionObject;
                scrollingGrid = activeLayout.ScrollingGrid;
                rocketAnimator = activeLayout.RocketAnimator;
                backgroundAnimator = activeLayout.BackgroundAnimator;
                historyContent = activeLayout.HistoryContent;
                playerRowsContent = activeLayout.PlayerRowsContent;
                betPanels = activeLayout.BetPanels;
            }
            else
            {
                ResolveFallbackReferences();
            }

            historyBadgePrefab ??= FindDeepGameObject("HistoryBadgeTemplate");
            playerRowPrefab ??= FindDeepGameObject("PlayerRowTemplate");
            BindActiveControls();
            InitializeActiveBetPanels(snapshots);
        }

        private void BindActiveControls()
        {
            if (backButton != null) backButton.onClick.AddListener(HandleBackPressed);
            if (currencyToggleButton != null) currencyToggleButton.onClick.AddListener(ToggleCurrency);

            if (betPanels == null)
            {
                return;
            }

            foreach (var panel in betPanels)
            {
                if (panel == null) continue;
                panel.BetAccepted += OnBetAccepted;
                panel.BetCancelled += OnBetCancelled;
            }
        }

        private void UnbindActiveControls()
        {
            if (backButton != null) backButton.onClick.RemoveListener(HandleBackPressed);
            if (currencyToggleButton != null) currencyToggleButton.onClick.RemoveListener(ToggleCurrency);

            if (betPanels == null)
            {
                return;
            }

            foreach (var panel in betPanels)
            {
                if (panel == null) continue;
                panel.BetAccepted -= OnBetAccepted;
                panel.BetCancelled -= OnBetCancelled;
            }
        }

        private void InitializeActiveBetPanels(IReadOnlyList<BetPanelSnapshot> snapshots)
        {
            if (betPanels == null || service == null)
            {
                return;
            }

            var activeCurrency = settings != null ? settings.ActiveCurrency : CurrencyMode.CC;
            for (var i = 0; i < betPanels.Length; i++)
            {
                var panel = betPanels[i];
                if (panel == null) continue;

                var panelId = i == 0 ? "BetPanelA" : "BetPanelB";
                panel.Initialize(panelId, service, activeCurrency);
                var snapshot = FindSnapshot(snapshots, panelId, i);
                if (snapshot.HasValue)
                {
                    panel.RestoreRuntimeState(snapshot.Value.State, snapshot.Value.Amount, snapshot.Value.Currency, snapshot.Value.Multiplier);
                }
            }
        }

        private List<BetPanelSnapshot> CapturePanelSnapshots()
        {
            var snapshots = new List<BetPanelSnapshot>();
            if (betPanels == null)
            {
                return snapshots;
            }

            for (var i = 0; i < betPanels.Length; i++)
            {
                var panel = betPanels[i];
                if (panel == null) continue;
                snapshots.Add(new BetPanelSnapshot(panel.PanelId, i, panel.State, panel.BetAmount, panel.Currency, panel.CurrentMultiplier));
            }

            return snapshots;
        }

        private static BetPanelSnapshot? FindSnapshot(IReadOnlyList<BetPanelSnapshot> snapshots, string panelId, int index)
        {
            if (snapshots == null)
            {
                return null;
            }

            foreach (var snapshot in snapshots)
            {
                if (snapshot.PanelId == panelId || snapshot.Index == index)
                {
                    return snapshot;
                }
            }

            return null;
        }

        private void OnCountdownTick(CrashCountdownEvent countdown)
        {
            currentPhase = CrashGamePhase.Preparation;
            lastCountdownSeconds = countdown.SecondsRemaining;
            currentStatus = $"NEXT ROUND IN {countdown.SecondsRemaining:0.0}s";
            currentMultiplierText = countdown.SecondsRemaining.ToString("0.0");
            currentMultiplierColor = Color.white;
            RenderTexts();

            foreach (var panel in betPanels)
            {
                panel?.OnCountdown();
            }

            if (rocketAnimator != null || backgroundAnimator != null)
            {
                rocketAnimator?.ShowCountdown(countdown.SecondsRemaining);
                backgroundAnimator?.ShowCountdown(countdown.SecondsRemaining);
            }
            else
            {
                ResetFlightVisuals();
            }
        }

        private void OnRoundStarted(CrashRoundStartedEvent started)
        {
            currentPhase = CrashGamePhase.Flight;
            currentStatus = "FLIGHT";
            RenderTexts();

            if (rocketAnimator != null || backgroundAnimator != null)
            {
                rocketAnimator?.ShowLaunch();
                backgroundAnimator?.ShowLaunch();
            }
            else
            {
                if (flameParticles != null) flameParticles.Play();
                if (explosionObject != null) explosionObject.SetActive(false);
            }

            foreach (var panel in betPanels)
            {
                panel?.OnRoundStarted();
            }
        }

        private void OnMultiplierUpdated(CrashMultiplierEvent update)
        {
            currentPhase = CrashGamePhase.Flight;
            lastMultiplierUpdate = update;
            currentMultiplierText = $"{update.Multiplier:F2}x";
            currentMultiplierColor = Color.white;
            RenderTexts();

            if (multiplierText != null)
            {
                multiplierText.transform.DOPunchScale(Vector3.one * 0.05f, 0.1f, 1, 0.5f);
            }

            if (scrollingGrid != null)
            {
                scrollingGrid.SetSpeedFactor((float)update.Multiplier);
            }

            if (backgroundAnimator != null)
            {
                backgroundAnimator.UpdateFlight(update.Multiplier);
            }

            if (rocketAnimator != null)
            {
                rocketAnimator.UpdateFlight(update);
            }
            else if (flameParticles != null)
            {
                var emission = flameParticles.emission;
                var intensity = Mathf.Clamp01((float)((update.Multiplier - 1.0) / 10.0));
                emission.rateOverTime = Mathf.Lerp(40f, 200f, intensity);
                if (!flameParticles.isPlaying) flameParticles.Play();
            }

            if (rocketAnimator == null)
            {
                UpdateRocketPosition(update);
            }

            foreach (var panel in betPanels)
            {
                panel?.OnMultiplierUpdated(update.Multiplier);
            }
        }

        private void UpdateRocketPosition(CrashMultiplierEvent update)
        {
            if (rocketTransform == null) return;

            rocketTransform.DOKill();
            const float duration = 0.05f;

            if (update.Multiplier < 1.1)
            {
                rocketTransform.DOAnchorPos(new Vector2(SY(-150f), SY(-100f)), duration).SetEase(Ease.OutSine);
                rocketTransform.DORotate(new Vector3(0f, 0f, 5f), duration).SetEase(Ease.OutSine);
            }
            else
            {
                var progress = Mathf.Clamp01((float)((update.Multiplier - 1.0) / 20.0));
                var noiseX = Mathf.Sin(Time.time * 3f) * 10f;
                var noiseY = Mathf.Cos(Time.time * 2.5f) * 15f;
                var targetPos = new Vector2(
                    Mathf.Lerp(SY(-150f), SY(250f), progress) + noiseX,
                    Mathf.Lerp(SY(-100f), SY(250f), Mathf.Sqrt(progress)) + noiseY);
                var targetRot = Mathf.Lerp(5f, 25f, progress);

                rocketTransform.DOAnchorPos(targetPos, duration).SetEase(Ease.Linear);
                rocketTransform.DORotate(new Vector3(0f, 0f, targetRot), duration).SetEase(Ease.Linear);
            }
        }

        private void OnRoundEnded(CrashRoundEndedEvent ended)
        {
            currentPhase = CrashGamePhase.Crashed;
            lastCrashPoint = ended.CrashPoint;
            currentStatus = "CRASHED";
            currentMultiplierText = $"CRASHED\n@ {ended.CrashPoint:F2}x";
            currentMultiplierColor = new Color(1f, 0.18f, 0.24f);
            RenderTexts();

            if (multiplierText != null)
            {
                multiplierText.transform.localScale = Vector3.one;
            }

            if (rocketAnimator != null || backgroundAnimator != null)
            {
                rocketAnimator?.ShowCrash();
                backgroundAnimator?.ShowCrash();
            }
            else
            {
                if (flameParticles != null) flameParticles.Stop();
                if (explosionObject != null) explosionObject.SetActive(true);
            }

            AddHistoryPill(ended.CrashPoint, true);
        }

        private void OnIntermissionStarted(int roundNonce)
        {
            currentPhase = CrashGamePhase.Intermission;
            currentStatus = "RESETTING";
            RenderTexts();
            rocketAnimator?.ShowIntermission();
            backgroundAnimator?.ShowIntermission();
        }

        private void OnBetResolved(CrashBetResolution resolution)
        {
            foreach (var panel in betPanels)
            {
                panel?.Resolve(resolution);
            }

            if (resolution.Won)
            {
                if (resolution.Currency == CurrencyMode.CC)
                {
                    OnBalanceChanged?.Invoke(resolution.Payout, 0.0);
                }
                else
                {
                    OnBalanceChanged?.Invoke(0.0, resolution.Payout);
                }
            }
        }

        private void OnBetAccepted(double amount, CurrencyMode activeCurrency)
        {
            if (activeCurrency == CurrencyMode.CC)
            {
                OnBalanceChanged?.Invoke(-amount, 0.0);
            }
            else
            {
                OnBalanceChanged?.Invoke(0.0, -amount);
            }
        }

        private void OnBetCancelled(double amount, CurrencyMode activeCurrency)
        {
            if (activeCurrency == CurrencyMode.CC)
            {
                OnBalanceChanged?.Invoke(amount, 0.0);
            }
            else
            {
                OnBalanceChanged?.Invoke(0.0, amount);
            }
        }

        private void HandleBackPressed()
        {
            OnRequestExit?.Invoke();
        }

        private void ToggleCurrency()
        {
            if (settings == null)
            {
                return;
            }

            var next = settings.ActiveCurrency == CurrencyMode.CC ? CurrencyMode.SC : CurrencyMode.CC;
            settings.SetCurrencyMode(next);

            foreach (var panel in betPanels)
            {
                panel?.SetCurrency(next);
            }
        }

        private void RenderPlayers(IReadOnlyList<CrashPlayerBet> bets)
        {
            latestPlayerBets.Clear();
            latestPlayerBets.AddRange(bets);
            RenderPlayersIntoActiveLayout();
        }

        private void RenderPlayersIntoActiveLayout()
        {
            if (playerRowsContent == null || playerRowPrefab == null)
            {
                return;
            }

            foreach (var row in playerRows)
            {
                if (row != null)
                {
                    DestroyRuntimeObject(row);
                }
            }

            playerRows.Clear();
            var count = Mathf.Min(8, latestPlayerBets.Count);
            for (var i = 0; i < count; i++)
            {
                var bet = latestPlayerBets[i];
                var row = Instantiate(playerRowPrefab, playerRowsContent);
                row.SetActive(true);
                SetText(row, "PlayerText", bet.PlayerName);
                SetText(row, "BetText", FormatAmount(bet.BetAmount));
                SetText(row, "MultiText", bet.IsCashedOut ? $"{bet.Multiplier:F2}x" : "-");
                SetText(row, "WinText", bet.WinAmount > 0 ? FormatAmount(bet.WinAmount) : "-");
                playerRows.Add(row);
            }
        }

        private void AddHistoryPill(double crashPoint, bool record)
        {
            if (record)
            {
                historyPoints.Add(crashPoint);
                if (historyPoints.Count > 30)
                {
                    historyPoints.RemoveAt(0);
                }
            }

            if (historyContent == null || historyBadgePrefab == null)
            {
                return;
            }

            var pill = Instantiate(historyBadgePrefab, historyContent);
            pill.SetActive(true);
            SetText(pill, "Text", $"{crashPoint:F2}x");
            var image = pill.GetComponent<Image>();
            if (image != null)
            {
                image.color = crashPoint >= 2.0 ? new Color(0.48f, 0.24f, 0.95f) : new Color(0.23f, 0.26f, 0.34f);
            }

            pill.transform.SetAsFirstSibling();
        }

        private void RebuildHistory()
        {
            if (historyContent == null || historyBadgePrefab == null)
            {
                return;
            }

            for (var i = historyContent.childCount - 1; i >= 0; i--)
            {
                DestroyRuntimeObject(historyContent.GetChild(i).gameObject);
            }

            for (var i = 0; i < historyPoints.Count; i++)
            {
                AddHistoryPill(historyPoints[i], false);
            }
        }

        private void ResetFlightVisuals()
        {
            if (rocketTransform != null)
            {
                rocketTransform.anchoredPosition = new Vector2(SY(-310f), SY(-190f));
                rocketTransform.localRotation = Quaternion.Euler(0f, 0f, -8f);
            }

            if (flameParticles != null) flameParticles.Stop();
            if (explosionObject != null) explosionObject.SetActive(false);
            if (scrollingGrid != null) scrollingGrid.SetSpeedFactor(1f);
        }

        private void RenderCurrentState()
        {
            RenderTexts();
            RenderBalance();
            RebuildHistory();
            RenderPlayersIntoActiveLayout();

            switch (currentPhase)
            {
                case CrashGamePhase.Preparation:
                    rocketAnimator?.ShowCountdown(lastCountdownSeconds);
                    backgroundAnimator?.ShowCountdown(lastCountdownSeconds);
                    break;
                case CrashGamePhase.Flight:
                    rocketAnimator?.UpdateFlight(lastMultiplierUpdate);
                    backgroundAnimator?.UpdateFlight(lastMultiplierUpdate.Multiplier);
                    if (rocketAnimator == null)
                    {
                        UpdateRocketPosition(lastMultiplierUpdate);
                    }
                    break;
                case CrashGamePhase.Crashed:
                    rocketAnimator?.ShowCrash();
                    backgroundAnimator?.ShowCrash();
                    break;
                case CrashGamePhase.Intermission:
                    rocketAnimator?.ShowIntermission();
                    backgroundAnimator?.ShowIntermission();
                    break;
            }
        }

        private void RenderTexts()
        {
            if (titleText != null) titleText.text = currentTitle;
            if (statusText != null) statusText.text = currentStatus;
            if (multiplierText != null)
            {
                multiplierText.color = currentMultiplierColor;
                multiplierText.text = currentMultiplierText;
            }
        }

        private void RenderBalance()
        {
            if (ccBalanceText != null) ccBalanceText.text = FormatAmount(balanceCc);
            if (scBalanceText != null) scBalanceText.text = balanceSc.ToString("0.00");
        }

        private void ResolveLayouts()
        {
            orientationManager ??= GetComponent<DynamicOrientationManager>();
            if (portraitLayout == null || landscapeLayout == null)
            {
                foreach (var layout in GetComponentsInChildren<CrashGameLayoutView>(true))
                {
                    if (layout.name == "Portrait_LayoutRoot") portraitLayout ??= layout;
                    if (layout.name == "Landscape_LayoutRoot") landscapeLayout ??= layout;
                }
            }
        }

        private CrashGameLayoutView ResolveActiveLayout()
        {
            ResolveLayouts();
            if (orientationManager != null && orientationManager.ActiveRoot != null)
            {
                var layout = orientationManager.ActiveRoot.GetComponent<CrashGameLayoutView>();
                if (layout != null)
                {
                    return layout;
                }
            }

            if (portraitLayout != null && portraitLayout.gameObject.activeInHierarchy) return portraitLayout;
            if (landscapeLayout != null && landscapeLayout.gameObject.activeInHierarchy) return landscapeLayout;
            return portraitLayout != null ? portraitLayout : landscapeLayout;
        }

        private void ResolveFallbackReferences()
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
            betPanels ??= GetComponentsInChildren<BetPanelController>(true);

            if (explosionObject == null)
            {
                explosionObject = FindDeepGameObject("Explosion");
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

        private GameObject FindDeepGameObject(string objectName)
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == objectName)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static void SetText(GameObject root, string childName, string value)
        {
            var texts = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in texts)
            {
                if (text.name == childName)
                {
                    text.text = value;
                    return;
                }
            }
        }

        private static string FormatAmount(double amount)
        {
            if (amount >= 1000000.0) return $"{amount / 1000000.0:0.#}M";
            if (amount >= 1000.0) return $"{amount / 1000.0:0.#}K";
            return amount.ToString("0");
        }

        private static void DestroyRuntimeObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private readonly struct BetPanelSnapshot
        {
            public BetPanelSnapshot(string panelId, int index, BetPanelState state, double amount, CurrencyMode currency, double multiplier)
            {
                PanelId = panelId;
                Index = index;
                State = state;
                Amount = amount;
                Currency = currency;
                Multiplier = multiplier;
            }

            public string PanelId { get; }
            public int Index { get; }
            public BetPanelState State { get; }
            public double Amount { get; }
            public CurrencyMode Currency { get; }
            public double Multiplier { get; }
        }
    }
}
