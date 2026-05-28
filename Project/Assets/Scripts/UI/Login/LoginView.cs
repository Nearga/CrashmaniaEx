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
            if (headerLoginButton == null) headerLoginButton = transform.Find("TopBar/LoginBtn")?.GetComponent<Button>();
            if (headerSignUpButton == null) headerSignUpButton = transform.Find("TopBar/Sign upBtn")?.GetComponent<Button>();
            if (joinNowButton == null) joinNowButton = transform.Find("Content/JoinNowBtn")?.GetComponent<Button>();
            if (playForFreeButton == null) playForFreeButton = transform.Find("Content/PlayForFreeBtn")?.GetComponent<Button>();
        }

        private void Start()
        {
            if (headerLoginButton != null) headerLoginButton.onClick.AddListener(() => LoginRequested?.Invoke());
            if (headerSignUpButton != null) headerSignUpButton.onClick.AddListener(() => SignUpRequested?.Invoke());
            if (joinNowButton != null) joinNowButton.onClick.AddListener(() => SignUpRequested?.Invoke());
            if (playForFreeButton != null) playForFreeButton.onClick.AddListener(() => SignUpRequested?.Invoke());
        }
    }
}
