using Crashmania.PureMvc.Mediators;
using Crashmania.UI.Login;
using PureMVC.Interfaces;
using UnityEngine;

namespace Crashmania.PureMvc.Scenes
{
    public sealed class LoginSceneController : MonoBehaviour, IPureMvcScene
    {
        [SerializeField] private LoginView loginView;

        private void Awake()
        {
            if (loginView == null) loginView = GetComponent<LoginView>();
        }

        public void Show(IFacade facade)
        {
            if (facade == null)
            {
                return;
            }

            Close(facade);

            if (loginView == null)
            {
                Debug.LogError("[LoginSceneController] Login scene is missing LoginView.");
                return;
            }

            facade.RegisterMediator(new LoginMediator(loginView));
            Debug.Log("[LoginSceneController] Login scene shown.");
        }

        public void Close(IFacade facade)
        {
            if (facade != null && facade.HasMediator(LoginMediator.Name))
            {
                facade.RemoveMediator(LoginMediator.Name);
            }
        }
    }
}
