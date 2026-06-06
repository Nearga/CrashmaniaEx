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
        [SerializeField] private Vector2 countdownNormalizedPosition = new(0.5f, 0.167f);
        [SerializeField] private Vector2 launchNormalizedPosition = new(0.527f, 0.259f);
        [SerializeField] private Vector2 flightTargetNormalizedPosition = new(0.805f, 0.637f);
        [SerializeField] private float countdownRotationDegrees = 0f;

        private Vector2 ResolvedCountdownPos => GetAnchoredPosition(countdownNormalizedPosition);
        private Vector2 ResolvedLaunchPos => GetAnchoredPosition(launchNormalizedPosition);
        private Vector2 ResolvedFlightTargetPos => GetAnchoredPosition(flightTargetNormalizedPosition);

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
            // Assuming anchors are Top-Left (0, 1):
            // x = normX * width
            // y = -(1f - normY) * height
            return new Vector2(normalizedPos.x * size.x, -(1f - normalizedPos.y) * size.y);
        }

        private Sequence idleSequence;
        private Sequence launchSequence;

        public bool HasSpineFallback => optionalSpineRoot != null;

        private void Awake()
        {
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
            rocketTransform.anchoredPosition = ResolvedCountdownPos;
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
                .Join(rocketTransform.DOAnchorPos(ResolvedLaunchPos, 0.28f).SetEase(Ease.OutBack))
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
                Mathf.Lerp(ResolvedLaunchPos.x, ResolvedFlightTargetPos.x, progress) + noiseX,
                Mathf.Lerp(ResolvedLaunchPos.y, ResolvedFlightTargetPos.y, lift) + noiseY
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
                .Append(rocketTransform.DOAnchorPosY(ResolvedCountdownPos.y + bobHeight, 0.65f).SetRelative(false).SetEase(Ease.InOutSine))
                .Join(rocketTransform.DORotate(new Vector3(0f, 0f, 4f), 0.65f).SetEase(Ease.InOutSine))
                .Append(rocketTransform.DOAnchorPosY(ResolvedCountdownPos.y - bobHeight, 0.65f).SetRelative(false).SetEase(Ease.InOutSine))
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
