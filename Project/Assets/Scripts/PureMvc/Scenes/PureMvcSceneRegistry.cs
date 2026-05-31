using System.Linq;
using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            var activeUnityScene = SceneManager.GetActiveScene();
            return Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                .OfType<IPureMvcScene>()
                .OrderByDescending(scene => scene is MonoBehaviour behaviour && behaviour.gameObject.scene == activeUnityScene)
                .FirstOrDefault(scene => scene is MonoBehaviour behaviour && behaviour.gameObject.scene.isLoaded);
        }
    }
}
