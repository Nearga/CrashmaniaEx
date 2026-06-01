using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Game
{
    public sealed class CrashBackgroundAnimator : MonoBehaviour
    {
        [SerializeField] private Graphic countdownBackground;
        [SerializeField] private Graphic flightSpaceBackground;
        [SerializeField] private RectTransform asteroids;
        [SerializeField] private RectTransform stars;
        [SerializeField] private RectTransform planet;
        [SerializeField] private RectTransform groundOrMoonLayer;
        [SerializeField] private Graphic speedLines;
        [SerializeField] private Graphic crashTint;

        private Vector2 asteroidBasePosition;
        private Vector2 starBasePosition;
        private Vector2 planetBasePosition;
        private Vector2 groundBasePosition;
        private bool isCountdownActive;
        private float countdownSecondsRemaining;

        private void Awake()
        {
            asteroidBasePosition = GetPosition(asteroids);
            starBasePosition = GetPosition(stars);
            planetBasePosition = GetPosition(planet);
            groundBasePosition = GetPosition(groundOrMoonLayer);
            ShowCountdown(8f);
        }

        private void Update()
        {
            if (isCountdownActive)
            {
                AnimateCountdownLayers();
                ApplyCountdownUrgency();
            }
        }

        private void OnDisable()
        {
            KillTweens();
        }

        public void ShowCountdown(float secondsRemaining)
        {
            countdownSecondsRemaining = secondsRemaining;

            if (!isCountdownActive)
            {
                isCountdownActive = true;
                KillTweens();
                Fade(countdownBackground, 1f, 0.15f);
                Fade(flightSpaceBackground, 0.75f, 0.2f);
                Fade(speedLines, 0f, 0.1f);
                Fade(crashTint, 0f, 0.1f);
            }

            AnimateCountdownLayers();
            ApplyCountdownUrgency();
        }

        public void ShowLaunch()
        {
            isCountdownActive = false;
            KillTweens();
            Fade(countdownBackground, 0.15f, 0.35f);
            Fade(flightSpaceBackground, 1f, 0.25f);
            Fade(speedLines, 0.24f, 0.25f);
            if (groundOrMoonLayer != null)
            {
                groundOrMoonLayer.DOAnchorPos(groundBasePosition + new Vector2(-24f, -32f), 0.45f).SetEase(Ease.OutSine);
            }
        }

        public void UpdateFlight(double multiplier)
        {
            isCountdownActive = false;
            var t = Mathf.Clamp01((float)((multiplier - 1.0) / 10.0));
            Fade(flightSpaceBackground, 1f, 0.08f);
            Fade(speedLines, Mathf.Lerp(0.18f, 0.48f, t), 0.08f);
            Fade(crashTint, 0f, 0.08f);

            SetPosition(stars, starBasePosition + new Vector2(-120f * t + Mathf.Sin(Time.time * 0.7f) * 8f, -36f * t));
            SetPosition(asteroids, asteroidBasePosition + new Vector2(-210f * t + Mathf.Sin(Time.time * 1.1f) * 18f, -82f * t));
            SetPosition(planet, planetBasePosition + new Vector2(-84f * t, -42f * t));
            SetPosition(groundOrMoonLayer, groundBasePosition + new Vector2(-170f * t, -90f * t));
        }

        public void ShowCrash()
        {
            isCountdownActive = false;
            KillTweens();
            Fade(speedLines, 0f, 0.12f);
            Fade(crashTint, 0.42f, 0.1f);
            if (asteroids != null)
            {
                asteroids.DOShakeAnchorPos(0.18f, 18f, 12, 80f);
            }

            if (planet != null)
            {
                planet.DOShakeAnchorPos(0.18f, 12f, 10, 70f);
            }
        }

        public void ShowIntermission()
        {
            isCountdownActive = false;
            Fade(crashTint, 0f, 0.35f);
            Fade(speedLines, 0f, 0.2f);
        }

        private void AnimateCountdownLayers()
        {
            var time = Time.time;
            SetPosition(stars, starBasePosition + new Vector2(Mathf.Sin(time * 0.28f) * 8f, Mathf.Cos(time * 0.22f) * 4f));
            SetPosition(asteroids, asteroidBasePosition + new Vector2(Mathf.Sin(time * 0.5f) * 16f, Mathf.Cos(time * 0.42f) * 8f));
            SetPosition(planet, planetBasePosition + new Vector2(Mathf.Sin(time * 0.18f) * 5f, Mathf.Cos(time * 0.16f) * 3f));
            SetPosition(groundOrMoonLayer, groundBasePosition + new Vector2(Mathf.Sin(time * 0.2f) * 10f, Mathf.Cos(time * 0.18f) * 5f));
        }

        private void ApplyCountdownUrgency()
        {
            var urgency = Mathf.Clamp01((5f - countdownSecondsRemaining) / 5f);
            if (countdownBackground != null)
            {
                countdownBackground.transform.localScale = Vector3.one * Mathf.Lerp(1f, 1.035f, urgency);
            }
        }

        private static Vector2 GetPosition(RectTransform target)
        {
            return target != null ? target.anchoredPosition : Vector2.zero;
        }

        private static void SetPosition(RectTransform target, Vector2 position)
        {
            if (target != null)
            {
                target.anchoredPosition = position;
            }
        }

        private static void Fade(Graphic graphic, float alpha, float duration)
        {
            if (graphic == null)
            {
                return;
            }

            graphic.DOKill();
            graphic.DOFade(Mathf.Clamp01(alpha), duration);
        }

        private void KillTweens()
        {
            countdownBackground?.DOKill();
            flightSpaceBackground?.DOKill();
            speedLines?.DOKill();
            crashTint?.DOKill();
            asteroids?.DOKill();
            stars?.DOKill();
            planet?.DOKill();
            groundOrMoonLayer?.DOKill();
        }
    }
}
