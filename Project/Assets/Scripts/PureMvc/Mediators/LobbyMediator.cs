using Crashmania.Core;
using Crashmania.Models;
using Crashmania.PureMvc.Notifications;
using Crashmania.PureMvc.Proxies;
using Crashmania.Services;
using Crashmania.UI.Lobby;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;

namespace Crashmania.PureMvc.Mediators
{
    public sealed class LobbyMediator : Mediator
    {
        public const string Name = "LobbyMediator";

        private LobbyView View => ViewComponent as LobbyView;

        public LobbyMediator(LobbyView view) : base(Name, view)
        {
        }

        public override void OnRegister()
        {
            if (View == null)
            {
                return;
            }

            View.CategorySelected += OnCategorySelected;
            View.GameSelected += OnGameSelected;
            View.ViewAllSelected += OnViewAllSelected;
            View.SearchChanged += OnSearchChanged;

            // Initialize target tab panel state from NavigationService
            var navigationService = ServiceLocator.Resolve<NavigationService>();
            if (navigationService != null)
            {
                View.ShowTab(navigationService.TargetTab);
            }

            // Instantly render cached catalog if available to avoid layout jumps
            var catalog = Facade.RetrieveProxy(CatalogProxy.Name) as CatalogProxy;
            if (catalog != null && catalog.Categories.Count > 0)
            {
                RenderFullCatalog(catalog);
            }
        }

        public override void OnRemove()
        {
            if (View == null)
            {
                return;
            }

            View.CategorySelected -= OnCategorySelected;
            View.GameSelected -= OnGameSelected;
            View.ViewAllSelected -= OnViewAllSelected;
            View.SearchChanged -= OnSearchChanged;
        }

        public override string[] ListNotificationInterests()
        {
            return new[] { LobbyNotifications.CatalogUpdated, LobbyNotifications.ShowTab };
        }

        public override void HandleNotification(INotification notification)
        {
            if (View == null)
            {
                return;
            }

            if (notification.Name == LobbyNotifications.ShowTab)
            {
                View.ShowTab(notification.Body as string);
                return;
            }

            if (notification.Body is LobbyDataResponse response)
            {
                View.Render(response);
                return;
            }

            var catalog = Facade.RetrieveProxy(CatalogProxy.Name) as CatalogProxy;
            if (catalog != null)
            {
                View.Render(new LobbyDataResponse
                {
                    Banners = new System.Collections.Generic.List<BannerModel>(catalog.Banners),
                    Categories = new System.Collections.Generic.List<CategoryModel>(catalog.Categories),
                    TopGames = new System.Collections.Generic.List<GameModel>(catalog.TopGames)
                });
            }
        }

        private void OnCategorySelected(string categoryId)
        {
            var catalog = Facade.RetrieveProxy(CatalogProxy.Name) as CatalogProxy;
            if (categoryId == "all")
            {
                RenderFullCatalog(catalog);
                return;
            }

            var category = catalog?.GetCategory(categoryId);
            if (category != null)
            {
                View.RenderSearchResults(category.Games);
            }
        }

        private void OnGameSelected(string gameId)
        {
            SendNotification(LobbyNotifications.LaunchGame, gameId);
            SendNotification(LobbyNotifications.ShowToast, "Loading game...");
        }

        private void OnViewAllSelected(string categoryId)
        {
            SendNotification(LobbyNotifications.ShowToast, "View all coming soon.");
        }

        private void OnSearchChanged(string query)
        {
            var catalog = Facade.RetrieveProxy(CatalogProxy.Name) as CatalogProxy;
            if (catalog == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                RenderFullCatalog(catalog);
                return;
            }

            View.RenderSearchResults(catalog.Search(query));
        }

        private void RenderFullCatalog(CatalogProxy catalog)
        {
            if (catalog == null)
            {
                return;
            }

            View.Render(new LobbyDataResponse
            {
                Banners = new System.Collections.Generic.List<BannerModel>(catalog.Banners),
                Categories = new System.Collections.Generic.List<CategoryModel>(catalog.Categories),
                TopGames = new System.Collections.Generic.List<GameModel>(catalog.TopGames)
            });
        }
    }
}
