using Crashmania.Models;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Game
{
    public sealed class CrashRocketAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform rocketTransform;
        [SerializeField] private Image rocketImage;
        [SerializeField] private ParticleSystem flameParticles;
        [SerializeField] private GameObject explosionObject;
        [SerializeField] private GameObject optionalSpineRoot;
        [SerializeField] private Vector2 countdownAnchoredPosition = new(566f, -673f);
        [SerializeField] private Vector2 launchAnchoredPosition = new(596f, -598f);
        [SerializeField] private Vector2 flightTargetAnchoredPosition = new(911f, -293f);
        [SerializeField] private float countdownRotationDegrees = 0f;

        private Sequence idleSequence;
        private Sequence launchSequence;

        public bool HasSpineFallback => optionalSpineRoot != null;

        private void Awake()
        {
            if (rocketTransform == null)
            {
                rocketTransform = GetComponent<RectTransform>();
            }

            if (rocketImage == null && rocketTransform != null)
            {
                rocketImage = rocketTransform.GetComponent<Image>();
            }

            if (flameParticles == null && rocketTransform != null)
            {
                flameParticles = rocketTransform.GetComponentInChildren<ParticleSystem>(true);
            }

            if (flameParticles != null)
            {
                var emission = flameParticles.emission;
                emission.rateOverTime = 0f;
                flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void OnDisable()
        {
            KillTweens();
        }

        public void ShowCountdown(float secondsRemaining)
        {
            if (rocketTransform == null)
            {
                return;
            }

            if (explosionObject != null)
            {
                explosionObject.SetActive(false);
            }

            if (rocketImage != null)
            {
                rocketImage.DOKill();
                rocketImage.enabled = true;
                rocketImage.color = Color.white;
            }

            if (optionalSpineRoot != null)
            {
                optionalSpineRoot.SetActive(false);
            }

            KillTweens();
            rocketTransform.DOKill();
            rocketTransform.anchoredPosition = countdownAnchoredPosition;
            rocketTransform.localRotation = Quaternion.Euler(0f, 0f, countdownRotationDegrees);
            rocketTransform.localScale = Vector3.one;

            if (flameParticles != null)
            {
                var emission = flameParticles.emission;
                emission.rateOverTime = 0f;
                flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void ShowLaunch()
        {
            if (rocketTransform == null)
            {
                return;
            }

            KillTweens();
            if (explosionObject != null)
            {
                explosionObject.SetActive(false);
            }

            if (flameParticles != null)
            {
                var emission = flameParticles.emission;
                emission.rateOverTime = 90f;
                flameParticles.Play();
            }

            launchSequence = DOTween.Sequence()
                .Join(rocketTransform.DOAnchorPos(launchAnchoredPosition, 0.28f).SetEase(Ease.OutBack))
                .Join(rocketTransform.DORotate(new Vector3(0f, 0f, 6f), 0.28f).SetEase(Ease.OutSine))
                .Join(rocketTransform.DOScale(new Vector3(1.04f, 0.96f, 1f), 0.12f).SetLoops(2, LoopType.Yoyo));
        }

        public void UpdateFlight(CrashMultiplierEvent update)
        {
            if (rocketTransform == null)
            {
                return;
            }

            idleSequence?.Kill();
            launchSequence?.Kill();
            rocketTransform.DOKill();

            var progress = Mathf.Clamp01((float)((update.Multiplier - 1.0) / 20.0));
            var lift = Mathf.Sqrt(progress);
            var noiseX = Mathf.Sin(Time.time * 3.2f) * Mathf.Lerp(6f, 18f, progress);
            var noiseY = Mathf.Cos(Time.time * 2.7f) * Mathf.Lerp(8f, 22f, progress);
            var targetPos = new Vector2(
                Mathf.Lerp(launchAnchoredPosition.x, flightTargetAnchoredPosition.x, progress) + noiseX,
                Mathf.Lerp(launchAnchoredPosition.y, flightTargetAnchoredPosition.y, lift) + noiseY
            );

            rocketTransform.DOAnchorPos(targetPos, 0.06f).SetEase(Ease.Linear);
            rocketTransform.DORotate(new Vector3(0f, 0f, Mathf.Lerp(5f, 27f, progress)), 0.06f).SetEase(Ease.Linear);
            rocketTransform.DOScale(Vector3.one * Mathf.Lerp(1f, 1.08f, progress), 0.06f).SetEase(Ease.Linear);

            if (flameParticles != null)
            {
                var emission = flameParticles.emission;
                var intensity = Mathf.Clamp01((float)((update.Multiplier - 1.0) / 10.0));
                emission.rateOverTime = Mathf.Lerp(60f, 230f, intensity);
                if (!flameParticles.isPlaying)
                {
                    flameParticles.Play();
                }
            }
        }

        public void ShowCrash()
        {
            KillTweens();

            if (rocketTransform != null)
            {
                rocketTransform.DOScale(0.72f, 0.16f).SetEase(Ease.InBack);
                rocketTransform.DORotate(new Vector3(0f, 0f, -28f), 0.16f).SetEase(Ease.InBack);
                if (rocketImage != null)
                {
                    rocketImage.DOFade(0f, 0.16f);
                }
            }

            if (flameParticles != null)
            {
                flameParticles.Stop();
            }

            if (explosionObject != null)
            {
                explosionObject.SetActive(true);
                explosionObject.transform.localScale = Vector3.one * 0.35f;
                explosionObject.transform.DOScale(1.05f, 0.2f).SetEase(Ease.OutBack);
            }
        }

        public void ShowIntermission()
        {
            if (flameParticles != null)
            {
                var emission = flameParticles.emission;
                emission.rateOverTime = 0f;
                flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void EnsureIdleTween(float secondsRemaining)
        {
            if (idleSequence != null && idleSequence.IsActive())
            {
                return;
            }

            var bobHeight = secondsRemaining <= 3f ? 18f : 10f;
            idleSequence = DOTween.Sequence()
                .Append(rocketTransform.DOAnchorPosY(countdownAnchoredPosition.y + bobHeight, 0.65f).SetRelative(false).SetEase(Ease.InOutSine))
                .Join(rocketTransform.DORotate(new Vector3(0f, 0f, 4f), 0.65f).SetEase(Ease.InOutSine))
                .Append(rocketTransform.DOAnchorPosY(countdownAnchoredPosition.y - bobHeight, 0.65f).SetRelative(false).SetEase(Ease.InOutSine))
                .Join(rocketTransform.DORotate(new Vector3(0f, 0f, countdownRotationDegrees), 0.65f).SetEase(Ease.InOutSine))
                .SetLoops(-1);
        }

        private void KillTweens()
        {
            idleSequence?.Kill();
            launchSequence?.Kill();
            if (rocketTransform != null)
            {
                rocketTransform.DOKill();
            }

            if (rocketImage != null)
            {
                rocketImage.DOKill();
            }
        }
    }
}
