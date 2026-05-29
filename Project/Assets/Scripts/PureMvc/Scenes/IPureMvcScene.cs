using PureMVC.Interfaces;

namespace Crashmania.PureMvc.Scenes
{
    public interface IPureMvcScene
    {
        void Show(IFacade facade);
        void Close(IFacade facade);
    }
}
