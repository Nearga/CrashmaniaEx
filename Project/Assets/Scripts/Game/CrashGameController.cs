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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Game
{
    public sealed class CrashGameController : MonoBehaviour, IGameController
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

        [Header("Lists")]
        [SerializeField] private RectTransform historyContent;
        [SerializeField] private GameObject historyBadgePrefab;
        [SerializeField] private RectTransform playerRowsContent;
        [SerializeField] private GameObject playerRowPrefab;

        [Header("Bets")]
        [SerializeField] private BetPanelController[] betPanels;

        private readonly List<GameObject> playerRows = new();
        private ICrashGameService service;
        private SettingsProxy settings;
        private double balanceCc;
        private double balanceSc;

        public event Action<double, double> OnBalanceChanged;
        public event Action OnRequestExit;

        private void Awake()
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

            if (explosionObject == null)
            {
                var explosion = FindDeep<Transform>("Explosion");
                explosionObject = explosion != null ? explosion.gameObject : null;
            }

            if (historyContent == null)
            {
                var history = FindDeep<RectTransform>("HistoryContent");
                historyContent = history;
            }

            if (playerRowsContent == null)
            {
                var rows = FindDeep<RectTransform>("PlayerRowsContent");
                playerRowsContent = rows;
            }

            if (betPanels == null || betPanels.Length == 0)
            {
                betPanels = GetComponentsInChildren<BetPanelController>(true);
            }

            if (backButton != null) backButton.onClick.AddListener(() => OnRequestExit?.Invoke());
            if (currencyToggleButton != null) currencyToggleButton.onClick.AddListener(ToggleCurrency);
        }

        public void Initialize(GameSession session, SettingsProxy settingsProxy)
        {
            settings = settingsProxy;
            service = ServiceLocator.Resolve<ICrashGameService>();
            var config = ServiceLocator.Resolve<AppConfig>();

            if (titleText != null)
            {
                titleText.text = string.IsNullOrWhiteSpace(session?.GameId) ? "CRASH" : session.GameId.ToUpperInvariant();
            }

            if (service != null)
            {
                Subscribe(service);
                service.StartLoop(config).Forget();
            }

            var activeCurrency = settings != null ? settings.ActiveCurrency : CurrencyMode.CC;
            for (var i = 0; i < betPanels.Length; i++)
            {
                var panel = betPanels[i];
                if (panel == null) continue;
                panel.Initialize(i == 0 ? "BetPanelA" : "BetPanelB", service, activeCurrency);
                panel.BetAccepted += OnBetAccepted;
                panel.BetCancelled += OnBetCancelled;
            }

            ResetFlightVisuals();
        }

        public void OnBalanceUpdated(double newCC, double newSC)
        {
            balanceCc = newCC;
            balanceSc = newSC;
            if (ccBalanceText != null) ccBalanceText.text = FormatAmount(balanceCc);
            if (scBalanceText != null) scBalanceText.text = balanceSc.ToString("0.00");
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

            foreach (var panel in betPanels)
            {
                if (panel == null) continue;
                panel.BetAccepted -= OnBetAccepted;
                panel.BetCancelled -= OnBetCancelled;
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
            if (statusText != null) statusText.text = $"NEXT ROUND IN {countdown.SecondsRemaining:0.0}s";
            if (multiplierText != null)
            {
                multiplierText.color = Color.white;
                multiplierText.text = countdown.SecondsRemaining.ToString("0.0");
            }

            foreach (var panel in betPanels)
            {
                panel?.OnCountdown();
            }

            ResetFlightVisuals();
        }

        private void OnRoundStarted(CrashRoundStartedEvent started)
        {
            if (statusText != null) statusText.text = "FLIGHT";
            if (flameParticles != null) flameParticles.Play();
            if (explosionObject != null) explosionObject.SetActive(false);

            foreach (var panel in betPanels)
            {
                panel?.OnRoundStarted();
            }
        }

private void OnMultiplierUpdated(CrashMultiplierEvent update)
        {
            if (multiplierText != null)
            {
                multiplierText.color = Color.white;
                multiplierText.text = $"{update.Multiplier:F2}x";
                var pulse = 1f + 0.04f * Mathf.Sin(Time.time * 20f);
                multiplierText.transform.localScale = new Vector3(pulse, pulse, 1f);
            }

            if (scrollingGrid != null)
            {
                scrollingGrid.SetSpeedFactor((float)update.Multiplier);
            }

            if (flameParticles != null)
            {
                var emission = flameParticles.emission;
                var intensity = Mathf.Clamp01((float)((update.Multiplier - 1.0) / 4.0));
                emission.rateOverTime = Mathf.Lerp(35f, 160f, intensity);
                if (!flameParticles.isPlaying)
                {
                    flameParticles.Play();
                }
            }

            if (rocketTransform != null)
            {
                var t = Mathf.Clamp01(update.ElapsedSeconds / 2.0f);
                var x = Mathf.Lerp(-330f, 130f, t);
                var y = Mathf.Lerp(-210f, 185f, t * t);
                rocketTransform.anchoredPosition = new Vector2(x, y);
                rocketTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-8f, 18f, t));
            }

            foreach (var panel in betPanels)
            {
                panel?.OnMultiplierUpdated(update.Multiplier);
            }
        }

        private void OnRoundEnded(CrashRoundEndedEvent ended)
        {
            if (statusText != null) statusText.text = "CRASHED";
            if (multiplierText != null)
            {
                multiplierText.color = new Color(1f, 0.18f, 0.24f);
                multiplierText.text = $"CRASHED\n@ {ended.CrashPoint:F2}x";
                multiplierText.transform.localScale = Vector3.one;
            }

            if (flameParticles != null) flameParticles.Stop();
            if (explosionObject != null) explosionObject.SetActive(true);
            AddHistoryPill(ended.CrashPoint);
        }

        private void OnIntermissionStarted(int roundNonce)
        {
            if (statusText != null) statusText.text = "RESETTING";
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
                rocketTransform.anchoredPosition = new Vector2(-310f, -190f);
                rocketTransform.localRotation = Quaternion.Euler(0f, 0f, -8f);
            }

            if (flameParticles != null) flameParticles.Stop();
            if (explosionObject != null) explosionObject.SetActive(false);
            if (scrollingGrid != null) scrollingGrid.SetSpeedFactor(1f);
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
