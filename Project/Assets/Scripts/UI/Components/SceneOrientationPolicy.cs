using UnityEngine;

namespace Crashmania.UI.Components
{
    public enum OrientationMode
    {
        ForcePortrait,
        PortraitOrLandscape
    }

    [DisallowMultipleComponent]
    public sealed class SceneOrientationPolicy : MonoBehaviour
    {
        [SerializeField] private OrientationMode orientationMode = OrientationMode.ForcePortrait;

        public OrientationMode Mode => orientationMode;

        private void Awake()
        {
            Apply();
        }

        public void Apply()
        {
            switch (orientationMode)
            {
                case OrientationMode.ForcePortrait:
                    LockPortrait();
                    break;
                case OrientationMode.PortraitOrLandscape:
                    AllowAutoRotation();
                    break;
            }
        }

        public static void LockPortrait()
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
        }

        public static void AllowAutoRotation()
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}