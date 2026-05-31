using Crashmania.Core;
using Crashmania.Models;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
using Crashmania.Services;
using Cysharp.Threading.Tasks;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

namespace Crashmania.PureMvc.Commands.Lobby
{
    public sealed class PurchaseStoreItemCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            var packageId = notification.Body as string;
            if (string.IsNullOrEmpty(packageId)) return;

            Purchase(packageId).Forget();
        }

        private async UniTaskVoid Purchase(string packageId)
        {
            try
            {
                var backend = ServiceLocator.Resolve<IBackendService>();
                if (backend == null) return;

                // TODO: Show confirmation modal here
                // For now, we go straight to purchase
                SendNotification(LobbyNotifications.ShowToast, "Processing purchase...");

                var result = await backend.PurchasePackage(packageId);
                if (result == null || !result.Success)
                {
                    SendNotification(LobbyNotifications.ShowToast, result?.ErrorMessage ?? "Purchase failed.");
                    return;
                }

                var balanceProxy = Facade.RetrieveProxy(BalanceProxy.Name) as BalanceProxy;
                balanceProxy?.Credit(result.CreditedCC, result.CreditedSC);

                SendNotification(LobbyNotifications.ShowToast, "Purchase successful!");
                SendNotification(LobbyNotifications.PurchaseComplete, result);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[PurchaseStoreItemCommand] Purchase failed: {exception}");
                SendNotification(LobbyNotifications.ShowToast, "Purchase error.");
            }
        }
    }
}
