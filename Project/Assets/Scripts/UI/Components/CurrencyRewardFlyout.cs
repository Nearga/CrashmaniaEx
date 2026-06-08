using System;
using System.Collections.Generic;
using Crashmania.Models;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Components
{
    [Serializable]
    public struct CurrencyRewardAnimationSettings
    {
        public float coinSize;
        public float scaleInDuration;
        public float launchStagger;
        public float flightDuration;
        public float fadeDuration;
        public float lateralSpread;
        public float arcHeight;

        public static CurrencyRewardAnimationSettings Default => new()
        {
            coinSize = 64f,
            scaleInDuration = 0.12f,
            launchStagger = 0.06f,
            flightDuration = 0.85f,
            fadeDuration = 0.18f,
            lateralSpread = 90f,
            arcHeight = 220f
        };
    }

    public sealed class CurrencyRewardFlyout : MonoBehaviour
    {
        [SerializeField] private RectTransform rewardLayer;
        [SerializeField] private Sprite ccCoinSprite;
        [SerializeField] private Sprite scCoinSprite;
        [SerializeField] private int minAmountOfCoins = 4;
        [SerializeField] private int maxAmountOfCoins = 8;
        [SerializeField] private float multiplierForMinAmountOfCoins = 1.3f;
        [SerializeField] private float multiplierForMaxAmountOfCoins = 5f;
        [SerializeField] private CurrencyRewardAnimationSettings defaultSettings;

        private readonly List<Image> coinPool = new();

        public int MinAmountOfCoins => minAmountOfCoins;
        public int MaxAmountOfCoins => maxAmountOfCoins;
        public int PoolCapacity => coinPool.Count;
        public int RequiredPoolCapacity => maxAmountOfCoins * 2;
        public Sprite CcCoinSprite => ccCoinSprite;
        public Sprite ScCoinSprite => scCoinSprite;

        private void Awake()
        {
            if (rewardLayer == null)
            {
                rewardLayer = transform as RectTransform;
            }

            if (defaultSettings.coinSize <= 0f)
            {
                defaultSettings = CurrencyRewardAnimationSettings.Default;
            }

            Prewarm();
        }

        public int CalculateCoinCount(double multiplier)
        {
            if (multiplier <= multiplierForMinAmountOfCoins)
            {
                return minAmountOfCoins;
            }

            if (multiplier >= multiplierForMaxAmountOfCoins)
            {
                return maxAmountOfCoins;
            }

            var progress = Mathf.InverseLerp(
                multiplierForMinAmountOfCoins,
                multiplierForMaxAmountOfCoins,
                (float)multiplier);
            return Mathf.RoundToInt(Mathf.Lerp(minAmountOfCoins, maxAmountOfCoins, progress));
        }

        public float Play(
            CurrencyMode currency,
            double payout,
            double multiplier,
            RectTransform source,
            RectTransform target,
            CurrencyRewardAnimationSettings settings,
            Action completionCallback = null)
        {
            if (source == null || target == null || rewardLayer == null)
            {
                completionCallback?.Invoke();
                return 0f;
            }

            if (settings.coinSize <= 0f)
            {
                settings = defaultSettings.coinSize > 0f
                    ? defaultSettings
                    : CurrencyRewardAnimationSettings.Default;
            }

            var coinCount = CalculateCoinCount(multiplier);
            var sourcePosition = ToLayerPosition(source);
            var targetPosition = ToLayerPosition(target);
            var sprite = currency == CurrencyMode.CC ? ccCoinSprite : scCoinSprite;
            var launched = 0;

            for (var i = 0; i < coinCount; i++)
            {
                var coin = GetAvailableCoin();
                if (coin == null)
                {
                    break;
                }

                AnimateCoin(coin, sprite, sourcePosition, targetPosition, settings, i, coinCount);
                launched++;
            }

            var totalDuration = settings.scaleInDuration
                + Mathf.Max(0, launched - 1) * settings.launchStagger
                + settings.flightDuration
                + settings.fadeDuration;

            if (completionCallback != null)
            {
                DOVirtual.DelayedCall(totalDuration, () => completionCallback());
            }

            return totalDuration;
        }

        public float Play(
            CurrencyMode currency,
            double payout,
            double multiplier,
            RectTransform source,
            RectTransform target,
            Action completionCallback = null)
        {
            return Play(currency, payout, multiplier, source, target, defaultSettings, completionCallback);
        }

        private void Prewarm()
        {
            var required = Mathf.Max(0, maxAmountOfCoins * 2);
            while (coinPool.Count < required)
            {
                var coinObject = new GameObject(
                    $"RewardCoin_{coinPool.Count:00}",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                var coin = coinObject.GetComponent<Image>();
                coin.transform.SetParent(rewardLayer, false);
                coin.raycastTarget = false;
                coin.preserveAspect = true;
                coin.gameObject.SetActive(false);
                coinPool.Add(coin);
            }
        }

        private Image GetAvailableCoin()
        {
            foreach (var coin in coinPool)
            {
                if (!coin.gameObject.activeSelf)
                {
                    return coin;
                }
            }

            return null;
        }

        private void AnimateCoin(
            Image coin,
            Sprite sprite,
            Vector2 source,
            Vector2 target,
            CurrencyRewardAnimationSettings settings,
            int index,
            int coinCount)
        {
            var rect = coin.rectTransform;
            coin.sprite = sprite;
            coin.color = Color.white;
            coin.gameObject.SetActive(true);
            coin.transform.SetAsLastSibling();
            rect.DOKill();
            rect.anchoredPosition = source;
            rect.sizeDelta = Vector2.one * settings.coinSize;
            rect.localScale = Vector3.zero;

            var normalized = coinCount <= 1 ? 0f : index / (float)(coinCount - 1) - 0.5f;
            var spread = normalized * settings.lateralSpread;
            var middle = Vector2.Lerp(source, target, 0.5f)
                + new Vector2(spread, settings.arcHeight + Mathf.Abs(spread) * 0.2f);
            var endApproach = Vector2.Lerp(source, target, 0.82f)
                + new Vector2(-spread * 0.25f, settings.arcHeight * 0.2f);

            var sequence = DOTween.Sequence();
            sequence.AppendInterval(index * settings.launchStagger);
            sequence.Append(rect.DOScale(Vector3.one, settings.scaleInDuration).SetEase(Ease.OutBack));
            sequence.Append(rect.DOLocalPath(
                    new[] { (Vector3)source, (Vector3)middle, (Vector3)endApproach, (Vector3)target },
                    settings.flightDuration,
                    PathType.CatmullRom,
                    PathMode.Ignore)
                .SetEase(Ease.InOutSine));
            sequence.Append(rect.DOScale(Vector3.zero, settings.fadeDuration).SetEase(Ease.InBack));
            sequence.Join(coin.DOFade(0f, settings.fadeDuration));
            sequence.OnComplete(() => coin.gameObject.SetActive(false));
        }

        private Vector2 ToLayerPosition(RectTransform target)
        {
            return rewardLayer.InverseTransformPoint(target.position);
        }
    }
}
