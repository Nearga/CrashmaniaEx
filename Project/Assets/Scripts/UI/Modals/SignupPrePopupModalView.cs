using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Modals
{
    public class SignupPrePopupModalView : MonoBehaviour
    {
        public Button continueButton;
        public Button closeButton;
        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => 
                    Crashmania.PureMvc.LobbyFacade.GetInstance().SendNotification(Crashmania.PureMvc.Notifications.LobbyNotifications.HideModal));
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(() =>
                {
                    var facade = Crashmania.PureMvc.LobbyFacade.GetInstance();
                    facade.SendNotification(Crashmania.PureMvc.Notifications.LobbyNotifications.HideModal);
                    facade.SendNotification(Crashmania.PureMvc.Notifications.LobbyNotifications.ShowModal, "SignupModal");
                });
            }
        }
    }
}
