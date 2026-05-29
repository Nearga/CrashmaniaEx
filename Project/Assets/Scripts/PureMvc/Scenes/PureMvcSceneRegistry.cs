using System.Linq;
using PureMVC.Interfaces;
using UnityEngine;

namespace Crashmania.PureMvc.Scenes
{
    public static class PureMvcSceneRegistry
    {
        private static IPureMvcScene activeScene;

        public static bool ShowActiveScene(IFacade facade)
        {
            CloseActiveScene(facade);

            var scene = FindActiveScene();
            if (scene == null)
            {
                return false;
            }

            scene.Show(facade);
            activeScene = scene;
            return true;
        }

        public static void CloseActiveScene(IFacade facade)
        {
            if (activeScene != null)
            {
                activeScene.Close(facade);
                activeScene = null;
                return;
            }

            FindActiveScene()?.Close(facade);
        }

        private static IPureMvcScene FindActiveScene()
        {
            return Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                .OfType<IPureMvcScene>()
                .FirstOrDefault(scene => scene is MonoBehaviour behaviour && behaviour.gameObject.scene.isLoaded);
        }
    }
}
