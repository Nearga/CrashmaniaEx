using Crashmania.PureMvc.Commands.Navigation;
using Crashmania.PureMvc.Notifications;
using PureMVC.Patterns.Facade;

namespace Crashmania.PureMvc
{
    public sealed class LobbyFacade : Facade
    {
        private bool started;

        static LobbyFacade()
        {
            instance = new LobbyFacade();
        }

        public static LobbyFacade GetInstance()
        {
            return instance as LobbyFacade;
        }

        public void Startup()
        {
            if (started)
            {
                return;
            }

            RegisterCommand(LobbyNotifications.NavigateTo, () => new Phase1NavigateCommand());
            started = true;
        }
    }
}
