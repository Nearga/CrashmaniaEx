using System;
using Crashmania.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Crashmania.UI.Modals
{
    public class LoginModalView : MonoBehaviour
    {
        public event Action CloseRequested;
        public event Action<LoginCredentials> LoginRequested;

        public TMP_InputField emailInput;
        public TMP_InputField passwordInput;
        public Button loginButton;
        public Button closeButton;
        public Button googleButton;
        public Button facebookButton;

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginClicked);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseClicked);
            if (loginButton != null) loginButton.onClick.RemoveListener(OnLoginClicked);
        }

        private void OnCloseClicked()
        {
            CloseRequested?.Invoke();
        }

        private void OnLoginClicked()
        {
            LoginRequested?.Invoke(new LoginCredentials
            {
                Email = emailInput != null ? emailInput.text : string.Empty,
                Password = passwordInput != null ? passwordInput.text : string.Empty,
                Provider = LoginProvider.Email
            });
        }
    }
}
