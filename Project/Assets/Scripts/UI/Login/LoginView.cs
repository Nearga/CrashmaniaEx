using System;
using Crashmania.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Login
{
    public sealed class LoginView : MonoBehaviour
    {
        public event Action LoginRequested;
        public event Action SignUpRequested;

        [SerializeField] private Button headerLoginButton;
        [SerializeField] private Button headerSignUpButton;
        [SerializeField] private Button joinNowButton;
        [SerializeField] private Button playForFreeButton;

        private static float ReferenceWidth => CanvasResolutionPolicy.ReferenceResolution.x;

        private void Awake()
        {
            SetupLayoutAndVisuals();

            if (headerLoginButton == null) headerLoginButton = transform.Find("ScrollRect/Viewport/Content/TopBar/LoginBtn")?.GetComponent<Button>();
            if (headerSignUpButton == null) headerSignUpButton = transform.Find("ScrollRect/Viewport/Content/TopBar/Sign upBtn")?.GetComponent<Button>();
            if (joinNowButton == null) joinNowButton = transform.Find("ScrollRect/Viewport/Content/Banner/JoinNowBtn")?.GetComponent<Button>();
            if (playForFreeButton == null) playForFreeButton = transform.Find("ScrollRect/Viewport/Content/Banner/PlayForFreeBtn")?.GetComponent<Button>();
        }

        private void Start()
        {
            if (headerLoginButton != null) headerLoginButton.onClick.AddListener(() => LoginRequested?.Invoke());
            if (headerSignUpButton != null) headerSignUpButton.onClick.AddListener(() => SignUpRequested?.Invoke());
            if (joinNowButton != null) joinNowButton.onClick.AddListener(() => SignUpRequested?.Invoke());
            if (playForFreeButton != null) playForFreeButton.onClick.AddListener(() => SignUpRequested?.Invoke());
        }

        private void SetupLayoutAndVisuals()
        {
            // 1. Root Screen Background — NOT raycastable (it's decorative only)
            Image rootImage = GetComponent<Image>();
            if (rootImage != null)
            {
                ColorUtility.TryParseHtmlString("#282b38", out Color bgColor);
                rootImage.color = bgColor;
                rootImage.raycastTarget = false;
            }

            // Disable raycastTarget on all static TMP texts (they never need input)
            foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
                tmp.raycastTarget = false;

            // 2. Drag Interceptor (transparent images on ScrollRect & Viewport)
            Transform scrollRectT = transform.Find("ScrollRect");
            if (scrollRectT != null)
            {
                Image scrollImage = scrollRectT.GetComponent<Image>();
                if (scrollImage == null) scrollImage = scrollRectT.gameObject.AddComponent<Image>();
                scrollImage.color = Color.clear;
                scrollImage.raycastTarget = true;

                Transform viewportT = scrollRectT.Find("Viewport");
                if (viewportT != null)
                {
                    Image viewportImage = viewportT.GetComponent<Image>();
                    if (viewportImage == null) viewportImage = viewportT.gameObject.AddComponent<Image>();
                    viewportImage.color = Color.clear;
                    viewportImage.raycastTarget = true;
                }
            }

            // 3. VerticalLayoutGroup and ContentSizeFitter Setup
            Transform contentT = transform.Find("ScrollRect/Viewport/Content");
            if (contentT != null)
            {
                VerticalLayoutGroup layoutGroup = contentT.GetComponent<VerticalLayoutGroup>();
                if (layoutGroup != null)
                {
                    layoutGroup.childForceExpandWidth = true;
                    layoutGroup.childForceExpandHeight = false;
                    layoutGroup.childControlWidth = true;
                    layoutGroup.childControlHeight = true;
                    layoutGroup.spacing = 0;
                    layoutGroup.padding = new RectOffset(0, 0, 0, 0);
                }

                ContentSizeFitter fitter = contentT.GetComponent<ContentSizeFitter>();
                if (fitter != null)
                {
                    fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }

                // 4. Configure TopBar Sizes & Logo Sprite
                Transform topBarT = contentT.Find("TopBar");
                if (topBarT != null)
                {
                    ConfigureLayoutElement(topBarT.gameObject, ReferenceWidth, 144);
                    Image topBarImage = topBarT.GetComponent<Image>();
                    if (topBarImage != null)
                    {
                        ColorUtility.TryParseHtmlString("#1a1d24", out Color barColor);
                        topBarImage.color = barColor;
                        topBarImage.raycastTarget = false; // background panel, not interactive
                    }

                    Transform logoT = topBarT.Find("Logo");
                    if (logoT != null)
                    {
                        Image logoImage = logoT.GetComponent<Image>();
                        if (logoImage != null)
                        {
                            Sprite logoSprite = Resources.Load<Sprite>("UI/Textures/Login/logo");
                            if (logoSprite != null)
                            {
                                logoImage.sprite = logoSprite;
                                logoImage.preserveAspect = true;
                                logoImage.color = Color.white;
                            }
                            logoImage.raycastTarget = false; // static asset
                        }
                        var rect = logoT.GetComponent<RectTransform>();
                        if (rect != null) rect.sizeDelta = new Vector2(107, 47);
                    }
                }

                // 5. Configure Banner Sizes & Sprite
                Transform bannerT = contentT.Find("Banner");
                if (bannerT != null)
                {
                    Image bannerImage = bannerT.GetComponent<Image>();
                    Sprite bannerSprite = Resources.Load<Sprite>("UI/Textures/Login/homepage-banner-mobile");
                    ConfigureLayoutElement(bannerT.gameObject, ReferenceWidth, HeightFromSpriteAspect(bannerSprite, ReferenceWidth, 1768f));
                    if (bannerImage != null)
                    {
                        if (bannerSprite != null)
                        {
                            bannerImage.sprite = bannerSprite;
                            bannerImage.preserveAspect = false;
                            bannerImage.color = Color.white;
                        }
                        bannerImage.raycastTarget = false; // background image, buttons inside are still raycastable
                    }
                }

                // 6. Configure Divider Sizes & Sprite
                Transform dividerT = contentT.Find("Divider");
                if (dividerT != null)
                {
                    Image dividerImage = dividerT.GetComponent<Image>();
                    Sprite dividerSprite = Resources.Load<Sprite>("UI/Textures/Login/hompage-divider-mobile");
                    ConfigureLayoutElement(dividerT.gameObject, ReferenceWidth, HeightFromSpriteAspect(dividerSprite, ReferenceWidth, 42f));
                    if (dividerImage != null)
                    {
                        if (dividerSprite != null)
                        {
                            dividerImage.sprite = dividerSprite;
                            dividerImage.preserveAspect = false;
                            dividerImage.color = Color.white;
                        }
                        dividerImage.raycastTarget = false; // static visual
                    }
                }

                // 7. Configure Footer Sizes & Color
                Transform footerT = contentT.Find("Footer");
                if (footerT != null)
                {
                    ConfigureLayoutElement(footerT.gameObject, ReferenceWidth, 250);
                    Image footerImage = footerT.GetComponent<Image>();
                    if (footerImage != null)
                    {
                        ColorUtility.TryParseHtmlString("#1a1d24", out Color footerColor);
                        footerImage.color = footerColor;
                        footerImage.raycastTarget = false; // static visual
                    }
                }

                // 8. Configure Carousels Sizes
                Transform top10T = contentT.Find("Top10Carousel");
                if (top10T != null) ConfigureLayoutElement(top10T.gameObject, ReferenceWidth, 360);

                Transform originalsT = contentT.Find("OriginalsCarousel");
                if (originalsT != null) ConfigureLayoutElement(originalsT.gameObject, ReferenceWidth, 360);

                // 9. Force layout rebuild so ContentSizeFitter computes correct scroll height immediately
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentT.GetComponent<RectTransform>());
            }
        }

        private static float HeightFromSpriteAspect(Sprite sprite, float width, float fallbackHeight)
        {
            if (sprite == null || sprite.rect.width <= 0f)
            {
                return fallbackHeight;
            }

            return width * sprite.rect.height / sprite.rect.width;
        }

        private void ConfigureLayoutElement(GameObject go, float width, float height)
        {
            LayoutElement le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = height;

            RectTransform rect = go.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(width, height);
            }
        }
    }
}
