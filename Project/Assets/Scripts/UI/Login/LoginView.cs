using System;
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

        private void Awake()
        {
            if (headerLoginButton == null) headerLoginButton = FindButton("ScrollRect/Viewport/Content/HeroSection/Header/LogInButton");
            if (headerSignUpButton == null) headerSignUpButton = FindButton("ScrollRect/Viewport/Content/HeroSection/Header/SignUpButton");
            if (joinNowButton == null) joinNowButton = FindButton("ScrollRect/Viewport/Content/HeroSection/Bonus/JoinNowButton");
            if (playForFreeButton == null) playForFreeButton = FindButton("ScrollRect/Viewport/Content/GameGallerySection/PlayForFreeButton");

            if (headerLoginButton == null || headerSignUpButton == null || joinNowButton == null || playForFreeButton == null)
            {
                Debug.LogError("[LoginView] Login scene is missing one or more required button bindings.");
            }
        }

        private void Start()
        {
            if (headerLoginButton != null) headerLoginButton.onClick.AddListener(OnLoginClicked);
            if (headerSignUpButton != null) headerSignUpButton.onClick.AddListener(OnSignUpClicked);
            if (joinNowButton != null) joinNowButton.onClick.AddListener(OnSignUpClicked);
            if (playForFreeButton != null) playForFreeButton.onClick.AddListener(OnSignUpClicked);
        }

        private void OnDestroy()
        {
            if (headerLoginButton != null) headerLoginButton.onClick.RemoveListener(OnLoginClicked);
            if (headerSignUpButton != null) headerSignUpButton.onClick.RemoveListener(OnSignUpClicked);
            if (joinNowButton != null) joinNowButton.onClick.RemoveListener(OnSignUpClicked);
            if (playForFreeButton != null) playForFreeButton.onClick.RemoveListener(OnSignUpClicked);
        }

        private Button FindButton(string path)
        {
            return transform.Find(path)?.GetComponent<Button>();
        }

        private void OnLoginClicked()
        {
            LoginRequested?.Invoke();
        }

        private void OnSignUpClicked()
        {
            SignUpRequested?.Invoke();
        }
    }
}
