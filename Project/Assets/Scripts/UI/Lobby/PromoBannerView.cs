using Crashmania.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Lobby
{
    public sealed class PromoBannerView : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button ctaButton;

        public event System.Action<string> CtaClicked;
        private BannerModel currentBanner;

        private void Awake()
        {
            if (image == null) image = transform.Find("Image")?.GetComponent<Image>() ?? GetComponent<Image>();
            if (titleText == null) titleText = transform.Find("Title")?.GetComponent<TMP_Text>();
            if (ctaButton == null) ctaButton = transform.Find("CTA")?.GetComponent<Button>() ?? GetComponentInChildren<Button>();

            if (ctaButton != null)
            {
                ctaButton.onClick.AddListener(OnCtaClick);
            }
        }

        private void OnDestroy()
        {
            if (ctaButton != null)
            {
                ctaButton.onClick.RemoveListener(OnCtaClick);
            }
        }

        public void Bind(BannerModel banner)
        {
            currentBanner = banner;
            if (titleText != null) titleText.text = banner != null ? banner.Title : string.Empty;
            if (image == null)
            {
                return;
            }

            var sprite = banner != null && !string.IsNullOrEmpty(banner.ImageResourcePath)
                ? Resources.Load<Sprite>(banner.ImageResourcePath)
                : null;
            image.sprite = sprite;
            image.color = sprite != null ? Color.white : new Color(0.08f, 0.08f, 0.1f, 1f);
            image.preserveAspect = true;
        }

        private void OnCtaClick()
        {
            if (currentBanner != null && !string.IsNullOrEmpty(currentBanner.LinkedGameId))
            {
                CtaClicked?.Invoke(currentBanner.LinkedGameId);
            }
        }
    }
}
