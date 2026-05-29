using Crashmania.Core;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
using Crashmania.Services;
using Cysharp.Threading.Tasks;
using PureMVC.Patterns.Command;
using UnityEngine;

namespace Crashmania.PureMvc.Commands.Lobby
{
    public sealed class LoadLobbyDataCommand : SimpleCommand
    {
        public override void Execute(PureMVC.Interfaces.INotification notification)
        {
            Load().Forget();
        }

        private async UniTaskVoid Load()
        {
            try
            {
                var backend = ServiceLocator.Resolve<IBackendService>();
                var response = await backend.GetLobbyData();
                var catalogProxy = Facade.RetrieveProxy(CatalogProxy.Name) as CatalogProxy;
                catalogProxy?.SetData(response);
                SendNotification(LobbyNotifications.CatalogUpdated, response);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[LoadLobbyDataCommand] Failed to load lobby data: {exception}");
                SendNotification(LobbyNotifications.ShowToast, "Failed to load lobby data.");
            }
        }
    }
}
