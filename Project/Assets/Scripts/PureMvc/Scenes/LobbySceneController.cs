using Crashmania.PureMvc.Mediators;
using Crashmania.PureMvc.Notifications;
using Crashmania.UI.Lobby;
using PureMVC.Interfaces;
using UnityEngine;

namespace Crashmania.PureMvc.Scenes
{
    public sealed class LobbySceneController : MonoBehaviour, IPureMvcScene
    {
        [SerializeField] private LobbyView lobbyView;

        public void Show(IFacade facade)
        {
            if (facade == null)
            {
                return;
            }

            Close(facade);

            if (lobbyView == null)
            {
                Debug.LogError("[LobbySceneController] Lobby scene is missing LobbyView.");
                return;
            }

            facade.RegisterMediator(new LobbyMediator(lobbyView));
            facade.SendNotification(LobbyNotifications.LoadLobbyData);
            Debug.Log("[LobbySceneController] Lobby scene shown.");
        }

        public void Close(IFacade facade)
        {
            if (facade != null && facade.HasMediator(LobbyMediator.Name))
            {
                facade.RemoveMediator(LobbyMediator.Name);
            }
        }
    }
}
