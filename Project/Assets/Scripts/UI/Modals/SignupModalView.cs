using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Crashmania.UI.Modals
{
    public class SignupModalView : MonoBehaviour
    {
        public event Action CloseRequested;
        public event Action PlayNowRequested;

        public TMP_InputField emailInput;
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public Button playNowButton;
        public Button closeButton;
        public Button googleButton;
        public Button facebookButton;

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (playNowButton != null)
            {
                playNowButton.onClick.AddListener(OnPlayNowClicked);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseClicked);
            if (playNowButton != null) playNowButton.onClick.RemoveListener(OnPlayNowClicked);
        }

        private void OnCloseClicked()
        {
            CloseRequested?.Invoke();
        }

        private void OnPlayNowClicked()
        {
            PlayNowRequested?.Invoke();
        }
    }
}
