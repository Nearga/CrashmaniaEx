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

        [Header("Layout Baselines")]
        [SerializeField] private Vector2 fallbackRocketCountdownNormalized = new(0.5f, 0.167f);
        [SerializeField] private Vector2 fallbackRocketLaunchNormalized = new(0.527f, 0.259f);
        [SerializeField] private Vector2 fallbackRocketFlightTargetNormalized = new(0.805f, 0.637f);
        [SerializeField] private float fallbackRocketCountdownRotation = 0f;

        private Vector2 ResolvedCountdownPos => GetAnchoredPosition(fallbackRocketCountdownNormalized);
        private Vector2 ResolvedLaunchPos => GetAnchoredPosition(fallbackRocketLaunchNormalized);
        private Vector2 ResolvedFlightTargetPos => GetAnchoredPosition(fallbackRocketFlightTargetNormalized);

        private Vector2 GetAnchoredPosition(Vector2 normalizedPos)
        {
            if (rocketTransform == null || rocketTransform.parent == null)
            {
                return Vector2.zero;
            }

            var parentRT = rocketTransform.parent as RectTransform;
            if (parentRT == null)
            {
                return Vector2.zero;
            }

            var size = parentRT.rect.size;
            return new Vector2(normalizedPos.x * size.x, -(1f - normalizedPos.y) * size.y);
        }

        [Header("Lists")]
        [SerializeField] private RectTransform historyContent;
        [SerializeField] private GameObject historyBadgePrefab;
        [SerializeField] private RectTransform playerRowsContent;
        [SerializeField] private GameObject playerRowPrefab;

        [Header("Bets")]
        [SerializeField] private BetPanelController[] betPanels;

        private readonly List<GameObject> playerRows = new();
        private ICrashGameService service;
        private RectTransform multiplierTextRect;
        private RectTransform statusTextRect;
        private Vector2 multiplierTextBasePosition;
        private Vector2 statusTextBasePosition;
        private int lastCountdownRoundNonce = int.MinValue;

        private readonly Dictionary<string, (double amount, CurrencyMode currency)> pendingBets = new();
        private double roundWinningsCc;
        private double roundWinningsSc;

        private Action<double, CurrencyMode> betAcceptedHandlerA;
        private Action<double, CurrencyMode> betCancelledHandlerA;
        private Action<double, CurrencyMode> betAcceptedHandlerB;
        private Action<double, CurrencyMode> betCancelledHandlerB;

        public event Action<double, double> OnBalanceChanged;

        private void Awake()
        {
            multiplierTextRect = multiplierText != null ? multiplierText.rectTransform : null;
            statusTextRect = statusText != null ? statusText.rectTransform : null;
            multiplierTextBasePosition = multiplierTextRect != null ? multiplierTextRect.anchoredPosition : Vector2.zero;
            statusTextBasePosition = statusTextRect != null ? statusTextRect.anchoredPosition : Vector2.zero;
        }

        public void Initialize(GameSession session, SettingsProxy settingsProxy)
        {
            service = ServiceLocator.Resolve<ICrashGameService>();
            var config = ServiceLocator.Resolve<AppConfig>();

            if (service != null)
            {
                Subscribe(service);
                service.StartLoop(config).Forget();
            }

            var activeCurrency = settingsProxy != null ? settingsProxy.ActiveCurrency : CurrencyMode.CC;
            for (var i = 0; i < betPanels.Length; i++)
            {
                var panel = betPanels[i];
                if (panel == null) continue;
                panel.Initialize(i == 0 ? "BetPanelA" : "BetPanelB", service, activeCurrency);
                string pid = panel.PanelId;
                if (i == 0)
                {
                    betAcceptedHandlerA = (amount, curr) => OnBetAccepted(pid, amount, curr);
                    betCancelledHandlerA = (amount, curr) => OnBetCancelled(pid, amount, curr);
                    panel.BetAccepted += betAcceptedHandlerA;
                    panel.BetCancelled += betCancelledHandlerA;
                }
                else
                {
                    betAcceptedHandlerB = (amount, curr) => OnBetAccepted(pid, amount, curr);
                    betCancelledHandlerB = (amount, curr) => OnBetCancelled(pid, amount, curr);
                    panel.BetAccepted += betAcceptedHandlerB;
                    panel.BetCancelled += betCancelledHandlerB;
                }
            }

            ResetFlightVisuals();
            ResetCounterTransforms();
        }

        public void OnBalanceUpdated(double newCC, double newSC)
        {
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

            for (var i = 0; i < betPanels.Length; i++)
            {
                var panel = betPanels[i];
                if (panel == null) continue;
                if (i == 0)
                {
                    if (betAcceptedHandlerA != null) panel.BetAccepted -= betAcceptedHandlerA;
                    if (betCancelledHandlerA != null) panel.BetCancelled -= betCancelledHandlerA;
                }
                else
                {
                    if (betAcceptedHandlerB != null) panel.BetAccepted -= betAcceptedHandlerB;
                    if (betCancelledHandlerB != null) panel.BetCancelled -= betCancelledHandlerB;
                }
                panel.ResetAutoplay();
            }

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

        private void OnCountdownTick(CrashCountdownEvent countdown)
        {
            if (countdown.RoundNonce != lastCountdownRoundNonce)
            {
                lastCountdownRoundNonce = countdown.RoundNonce;
                ResetCounterTransforms();
            }

            if (statusText != null) statusText.text = $"NEXT ROUND IN";
            if (multiplierText != null)
            {
                multiplierText.color = Color.white;
                multiplierText.text = countdown.SecondsRemaining.ToString("0.0");
            }

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
            ResetCounterTransforms();
            if (statusText != null) statusText.text = "FLIGHT";
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

            // Debit active bets from balance exactly at round start
            double totalCc = 0;
            double totalSc = 0;
            foreach (var bet in pendingBets.Values)
            {
                if (bet.currency == CurrencyMode.CC)
                {
                    totalCc += bet.amount;
                }
                else
                {
                    totalSc += bet.amount;
                }
            }

            if (totalCc > 0 || totalSc > 0)
            {
                OnBalanceChanged?.Invoke(-totalCc, -totalSc);
            }

            pendingBets.Clear();

            // Reset winnings accumulator
            roundWinningsCc = 0;
            roundWinningsSc = 0;
        }

        private void OnMultiplierUpdated(CrashMultiplierEvent update)
        {
            if (multiplierText != null)
            {
                multiplierText.text = $"{update.Multiplier:F2}x";
                multiplierText.transform.DOKill();
                multiplierText.transform.localScale = Vector3.one;
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

            // Kill any existing tween to avoid conflicts
            rocketTransform.DOKill();

            float duration = 0.05f; // Match multiplier tick rate

            if (update.Multiplier < 1.1)
            {
                // Launch phase
                rocketTransform.DOAnchorPos(ResolvedLaunchPos, duration).SetEase(Ease.OutSine);
                rocketTransform.DORotate(new Vector3(0f, 0f, 5f), duration).SetEase(Ease.OutSine);
            }
            else
            {
                // Flight phase: Dynamic mapping of multiplier to viewport space
                float progress = Mathf.Clamp01((float)((update.Multiplier - 1.0) / 20.0)); // Reach top-right at 20x
                
                // Add some "floaty" noise using Sine
                float noiseX = Mathf.Sin(Time.time * 3f) * 10f;
                float noiseY = Mathf.Cos(Time.time * 2.5f) * 15f;

                Vector2 targetPos = new Vector2(
                    Mathf.Lerp(ResolvedLaunchPos.x, ResolvedFlightTargetPos.x, progress) + noiseX,
                    Mathf.Lerp(ResolvedLaunchPos.y, ResolvedFlightTargetPos.y, Mathf.Sqrt(progress)) + noiseY
                );

                float targetRot = Mathf.Lerp(5f, 25f, progress);

                rocketTransform.DOAnchorPos(targetPos, duration).SetEase(Ease.Linear);
                rocketTransform.DORotate(new Vector3(0f, 0f, targetRot), duration).SetEase(Ease.Linear);
            }
        }

        private void OnRoundEnded(CrashRoundEndedEvent ended)
        {
            if (statusText != null) statusText.text = "";
            if (multiplierText != null)
            {
                multiplierText.transform.DOKill();
                multiplierText.color = new Color(1f, 0.18f, 0.24f);
                multiplierText.text = $"CRASHED\n@ {ended.CrashPoint:F2}x";
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

            AddHistoryPill(ended.CrashPoint);

            // Credit winnings exactly at round end
            if (roundWinningsCc > 0 || roundWinningsSc > 0)
            {
                OnBalanceChanged?.Invoke(roundWinningsCc, roundWinningsSc);
            }
        }

        private void OnIntermissionStarted(int roundNonce)
        {
            if (statusText != null) statusText.text = "RESETTING";
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
                    roundWinningsCc += resolution.Payout;
                }
                else
                {
                    roundWinningsSc += resolution.Payout;
                }
            }
        }

        private void OnBetAccepted(string panelId, double amount, CurrencyMode activeCurrency)
        {
            pendingBets[panelId] = (amount, activeCurrency);
        }

        private void OnBetCancelled(string panelId, double amount, CurrencyMode activeCurrency)
        {
            pendingBets.Remove(panelId);
        }

        private void RenderPlayers(IReadOnlyList<CrashPlayerBet> bets)
        {
            if (playerRowsContent == null || playerRowPrefab == null)
            {
                return;
            }

            foreach (var row in playerRows)
            {
                Destroy(row);
            }

            playerRows.Clear();
            var count = Mathf.Min(8, bets.Count);
            for (var i = 0; i < count; i++)
            {
                var bet = bets[i];
                var row = Instantiate(playerRowPrefab, playerRowsContent);
                row.SetActive(true);
                SetText(row, "PlayerText", bet.PlayerName);
                SetText(row, "BetText", FormatAmount(bet.BetAmount));
                SetText(row, "MultiText", bet.IsCashedOut ? $"{bet.Multiplier:F2}x" : "-");
                SetText(row, "WinText", bet.WinAmount > 0 ? FormatAmount(bet.WinAmount) : "-");
                playerRows.Add(row);
            }
        }

        private void AddHistoryPill(double crashPoint)
        {
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

        private void ResetFlightVisuals()
        {
            if (rocketTransform != null)
            {
                rocketTransform.DOKill();
                rocketTransform.anchoredPosition = ResolvedCountdownPos;
                rocketTransform.localRotation = Quaternion.Euler(0f, 0f, fallbackRocketCountdownRotation);
                rocketTransform.localScale = Vector3.one;
            }

            if (flameParticles != null) flameParticles.Stop();
            if (explosionObject != null) explosionObject.SetActive(false);
            if (scrollingGrid != null) scrollingGrid.SetSpeedFactor(1f);
        }

        private void ResetCounterTransforms()
        {
            if (multiplierTextRect != null)
            {
                multiplierTextRect.DOKill();
                multiplierTextRect.anchoredPosition = multiplierTextBasePosition;
                multiplierTextRect.localRotation = Quaternion.identity;
                multiplierTextRect.localScale = Vector3.one;
            }

            if (statusTextRect != null)
            {
                statusTextRect.DOKill();
                statusTextRect.anchoredPosition = statusTextBasePosition;
                statusTextRect.localRotation = Quaternion.identity;
                statusTextRect.localScale = Vector3.one;
            }
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
    }
}
