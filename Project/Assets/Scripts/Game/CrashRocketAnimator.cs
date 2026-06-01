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
        [SerializeField] private RectTransform rocketGlow;
        [SerializeField] private Image rocketGlowImage;
        [SerializeField] private ParticleSystem flameParticles;
        [SerializeField] private GameObject explosionObject;
        [SerializeField] private GameObject optionalSpineRoot;
        [SerializeField] private Vector2 countdownAnchoredPosition = new(490f, -590f);
        [SerializeField] private Vector2 launchAnchoredPosition = new(520f, -515f);
        [SerializeField] private Vector2 flightTargetAnchoredPosition = new(835f, -210f);
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

            SetGlowVisible(true, 0.18f);
            rocketTransform.DOKill();
            rocketTransform.anchoredPosition = countdownAnchoredPosition;
            rocketTransform.localRotation = Quaternion.Euler(0f, 0f, countdownRotationDegrees);
            rocketTransform.localScale = Vector3.one;
            EnsureIdleTween(secondsRemaining);

            if (flameParticles != null)
            {
                var emission = flameParticles.emission;
                emission.rateOverTime = 28f;
                if (!flameParticles.isPlaying)
                {
                    flameParticles.Play();
                }
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

            SetGlowVisible(true, 0.38f);
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
            SetGlowVisible(true, Mathf.Lerp(0.2f, 0.55f, progress));

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

            SetGlowVisible(false, 0f);
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
                flameParticles.Stop();
            }

            SetGlowVisible(false, 0f);
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

        private void SetGlowVisible(bool visible, float alpha)
        {
            if (rocketGlow == null)
            {
                return;
            }

            rocketGlow.gameObject.SetActive(visible || alpha > 0f);
            rocketGlow.DOKill();
            rocketGlow.localScale = Vector3.one;
            if (visible)
            {
                rocketGlow.DOScale(1.18f, 0.35f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }

            if (rocketGlowImage != null)
            {
                var color = rocketGlowImage.color;
                color.a = Mathf.Clamp01(alpha);
                rocketGlowImage.color = color;
            }
        }

        private void KillTweens()
        {
            idleSequence?.Kill();
            launchSequence?.Kill();
            if (rocketTransform != null)
            {
                rocketTransform.DOKill();
            }

            if (rocketGlow != null)
            {
                rocketGlow.DOKill();
            }

            if (rocketImage != null)
            {
                rocketImage.DOKill();
            }
        }
    }
}
