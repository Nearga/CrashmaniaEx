using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Crashmania.UI.Modals
{
    public class SignupModalView : MonoBehaviour
    {
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
                closeButton.onClick.AddListener(() => 
                    Crashmania.PureMvc.LobbyFacade.GetInstance().SendNotification(Crashmania.PureMvc.Notifications.LobbyNotifications.HideModal));
            }

            if (playNowButton != null)
            {
                playNowButton.onClick.AddListener(() =>
                {
                    var facade = Crashmania.PureMvc.LobbyFacade.GetInstance();
                    facade.SendNotification(Crashmania.PureMvc.Notifications.LobbyNotifications.HideModal);
                    facade.SendNotification(Crashmania.PureMvc.Notifications.LobbyNotifications.NavigateTo, "Lobby");
                    facade.SendNotification(Crashmania.PureMvc.Notifications.LobbyNotifications.ShowToast, "Successfully Registered!");
                });
            }
        }
    }
}
