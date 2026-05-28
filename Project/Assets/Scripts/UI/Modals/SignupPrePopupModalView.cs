using System;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Modals
{
    public class SignupPrePopupModalView : MonoBehaviour
    {
        public event Action CloseRequested;
        public event Action ContinueRequested;

        public Button continueButton;
        public Button closeButton;

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseClicked);
            if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        private void OnCloseClicked()
        {
            CloseRequested?.Invoke();
        }

        private void OnContinueClicked()
        {
            ContinueRequested?.Invoke();
        }
    }
}
