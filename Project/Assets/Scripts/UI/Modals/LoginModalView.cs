using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Crashmania.PureMvc.Notifications;
using PureMVC.Patterns.Facade;

namespace Crashmania.UI.Modals
{
    public class LoginModalView : MonoBehaviour
    {
        public TMP_InputField emailInput;
        public TMP_InputField passwordInput;
        public Button loginButton;
        public Button closeButton;
        public Button googleButton;
        public Button facebookButton;
    }
}
